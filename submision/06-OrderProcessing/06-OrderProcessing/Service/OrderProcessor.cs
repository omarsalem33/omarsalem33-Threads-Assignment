using System.Diagnostics;
using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;
using _06_OrderProcessing.Service.Interfaces;

namespace _06_OrderProcessing.Service;

public sealed class OrderProcessor
{
    private readonly CustomerService _customerService;
    private readonly InventoryService _inventoryService;
    private readonly PricingService _pricingService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly OrderRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly ProductRepository _productRepository;
    private readonly Action<int>? _threadObserved;

    public OrderProcessor(CustomerService customerService, InventoryService inventoryService, PricingService pricingService, IPaymentGateway paymentGateway, OrderRepository orderRepository, IEmailService emailService, ProductRepository productRepository , Action<int>? threadObserved =null )
    {
        _customerService = customerService;
        _inventoryService = inventoryService;
        _pricingService = pricingService;
        _paymentGateway = paymentGateway;
        _orderRepository = orderRepository;
        _emailService = emailService;
        _productRepository = productRepository;
        _threadObserved = threadObserved;

    }

    public async Task<bool> ProcessOrderAsync(Order order, CancellationToken ct)
    {
        order.FailureReason = null;
        var timer = Stopwatch.StartNew();
        Customer customer;

        try
        {
            customer = await _customerService.GetCustomerAsync(order.CustomerId, ct);
        }
        catch (CustomerNotFoundException ex)
        {
            await RejectAndSaveAsync(order, ex.Message, ct);
            LogStep(order.Id, 1, "Customer", timer, $"REJECTED: {ex.Message}");
            return false;
        }

        if (!customer.IsActive)
        {
            const string reason = "Customer is inactive";
            await RejectAndSaveAsync(order, reason, ct);
            LogStep(order.Id, 1, "Customer", timer, $"REJECTED: {reason}");
            return false;
        }

        LogStep(order.Id, 1, "Customer", timer, "OK");

        timer = Stopwatch.StartNew();

        bool stockReserved = await _inventoryService.ReserveStockAsync(order, ct);

        if (!stockReserved)
        {
            string productName = await FindInsufficientProductNameAsync(order, ct);
            string reason = $"Insufficient stock for '{productName}'";

            await RejectAndSaveAsync(order, reason, ct);
            LogStep(order.Id, 2, "Inventory", timer, $"REJECTED: {reason}");
            return false;
        }

        LogStep(order.Id, 2, "Inventory", timer, "OK");

        timer = Stopwatch.StartNew();

        try
        {
            await _pricingService.CalculateTotalAsync(order, ct);
        }
        catch (InvalidCouponException ex)
        {
            await _inventoryService.ReleaseStockAsync(order, CancellationToken.None);

            await RejectAndSaveAsync(order, ex.Message, ct);
            LogStep(order.Id, 3, "Pricing", timer, $"REJECTED: {ex.Message}");
            return false;
        }

        LogStep(order.Id, 3, "Pricing", timer, "OK");

        timer = Stopwatch.StartNew();
        PaymentResult paymentResult;

        using (var paymentCts = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            paymentCts.CancelAfter(TimeSpan.FromSeconds(3));

            try
            {
                paymentResult = await _paymentGateway.ProcessPaymentAsync(order.Id, order.TotalAmount, paymentCts.Token);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                await _inventoryService.ReleaseStockAsync(order, CancellationToken.None);
                LogStep(order.Id, 4, "Payment", timer, "RUN CANCELLED");
                throw;
            }
            catch (OperationCanceledException)
            {
                const string reason = "Payment gateway timeout (3s)";

                await _inventoryService.ReleaseStockAsync(order, CancellationToken.None);

                order.Status = OrderStatus.PaymentFailed;
                order.FailureReason = reason;
                await _orderRepository.SaveAsync(order, CancellationToken.None);

                LogStep(order.Id, 4, "Payment", timer, $"TIMEOUT: {reason}");
                return false;
            }
        }

        if (!paymentResult.Success)
        {
            await _inventoryService.ReleaseStockAsync(order, CancellationToken.None);

            string reason = paymentResult.Message ?? "Card declined by issuer";
            order.Status = OrderStatus.PaymentFailed;
            order.FailureReason = reason;

            await _orderRepository.SaveAsync(order, ct);

            LogStep(order.Id, 4, "Payment", timer, $"DECLINED: {reason}");
            return false;
        }

        order.TransactionId = paymentResult.TransactionId;
        LogStep(order.Id, 4, "Payment", timer, $"OK ({order.TransactionId})");

        timer = Stopwatch.StartNew();
        order.Status = OrderStatus.Completed;

        try
        {
            await _orderRepository.SaveAsync(order, ct);
        }
        catch (DatabaseWriteException)
        {
            if (order.TransactionId is not null)
            {
                await _paymentGateway.RefundAsync(order.TransactionId);
            }

            await _inventoryService.ReleaseStockAsync(order, CancellationToken.None);

            order.Status = OrderStatus.Failed;
            order.FailureReason = $"Database save failed after payment; payment " +
                                  $"{order.TransactionId} refunded";

            await _orderRepository.SaveAsync(order, CancellationToken.None);

            LogStep(order.Id, 5, "Save", timer, $"FAILED; payment {order.TransactionId} refunded");

            return false;
        }

        LogStep(order.Id, 5, "Save", timer, "OK");
        timer = Stopwatch.StartNew();

        try
        {
            await SendConfirmationWithRetryAsync(order, customer);
        }
        catch (Exception ex)
        {
            LogStep(order.Id, 6, "Email", timer, $"ERROR: {ex.Message}; order remains Completed");
            return true;
        }

        LogStep(order.Id, 6, "Email", timer, "DONE");
        return order.Status == OrderStatus.Completed;
    }

    private async Task RejectAndSaveAsync(Order order, string reason, CancellationToken ct)
    {
        order.Status = OrderStatus.Rejected;
        order.FailureReason = reason;

        await _orderRepository.SaveAsync(order, ct);
    }

    private async Task<string> FindInsufficientProductNameAsync(Order order, CancellationToken ct)
    {
        foreach (var item in order.Items)
        {
            Product? product = await _productRepository.GetByIdAsync(item.ProductId, ct);
            if (product is null || product.StockQuantity < item.Quantity)
            {
                return product?.Name ?? $"Product #{item.ProductId}";
            }
        }

        return "requested product";
    }

    private async Task SendConfirmationWithRetryAsync(Order order, Customer customer)
    {
        string? lastError = null;

        for (int attempt = 1; attempt <= 4; attempt++)
        {
            var timer = Stopwatch.StartNew();

            try
            {
                await _emailService.SendConfirmationAsync(order, customer);
                return;
            }
            catch (SmtpUnavailableException ex)
            {
                lastError = ex.Message;

                LogStep(order.Id, 6, $"Email attempt {attempt}/4", timer, $"FAILED: {ex.Message}");

                if (attempt < 4)
                {
                    int delayMilliseconds = attempt switch
                    {
                        1 => 500,
                        2 => 1000,
                        _ => 2000
                    };

                    await Task.Delay(delayMilliseconds);
                }
            }
        }

        var retryEmail = new EmailMessage
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            To = customer.Email,
            Subject = $"Order #{order.Id} confirmation",
            Body = $"Your order #{order.Id} is confirmed. Total: {order.TotalAmount:C}",
            SentAt = DateTime.Now,
            Attempts = 4,
            LastError = lastError
        };

        string retryPath = DataFiles.GetFullPath(DataFiles.EmailRetry);

        await JsonFileStore.UpdateAsync<List<EmailMessage>>(
            retryPath, emails =>
            {
                emails.Add(retryEmail);
                return emails;
            },
            CancellationToken.None);

        Console.WriteLine(
            $"Email for Order #{order.Id} queued for retry later");
    }

    private void LogStep(int orderId, int step, string name, Stopwatch timer, string result)
    {
        _threadObserved?.Invoke(Environment.CurrentManagedThreadId);

        timer.Stop();

        Console.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] " +
            $"[Thread {Environment.CurrentManagedThreadId}] " +
            $"Order #{orderId} -> Step {step}/6 {name} ... {result} " +
            $"in {timer.ElapsedMilliseconds} ms");
    }
}