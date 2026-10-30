using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing.Service;

public sealed class SyncOrderProcessor
{
    private readonly SyncOrderValidator _validator;
    private readonly SyncInventoryService _inventory;
    private readonly SyncPricingService _pricing;
    private readonly SyncOrderRepository _orders;
    private readonly SyncCustomerRepository _customers;
    private readonly SyncProductRepository _products;
    

    public SyncOrderProcessor(SyncOrderValidator validator, SyncInventoryService inventory, SyncPricingService pricing, SyncOrderRepository orders, SyncCustomerRepository customers, SyncProductRepository products)
    {
        _validator = validator;
        _inventory = inventory;
        _pricing = pricing;
        _orders = orders;
        _customers = customers;
        _products = products;
    }

    public bool ProcessOrder(Order order)
    {
        _validator.ValidateOrder(order);

        if (order.Status == OrderStatus.Rejected)
        {
            _orders.Save(order);
            return false;
        }

        if (!_inventory.ReserveStock(order))
        {
            string productName = FindInsufficientProductName(order);
            order.Status = OrderStatus.Rejected;
            order.FailureReason = $"Insufficient stock for '{productName}'";

            _orders.Save(order);
            return false;
        }

        try
        {
            _pricing.CalculateTotal(order);
        }
        catch (InvalidCouponException ex)
        {
            _inventory.ReleaseStock(order);

            order.Status = OrderStatus.Rejected;
            order.FailureReason = ex.Message;
            _orders.Save(order);

            return false;
        }

        Thread.Sleep(700);

        string transactionId = $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";

        var payment = new PaymentTransaction
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            TransactionId = transactionId,
            Status = PaymentStatus.Captured,
            CreatedAt = DateTime.Now
        };

        JsonFileStore.Update<List<PaymentTransaction>>(
            DataFiles.GetFullPath(DataFiles.Payments),
            payments =>
            {
                payments.Add(payment);
                return payments;
            });

        order.TransactionId = transactionId;
        order.Status = OrderStatus.Completed;
        order.FailureReason = null;
        _orders.Save(order);

        Customer? customer = _customers.GetById(order.CustomerId);

        if (customer is not null)
        {
            Thread.Sleep(500);

            var email = new EmailMessage
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                To = customer.Email,
                Subject = $"Order #{order.Id} confirmation",
                Body = $"Your order #{order.Id} is confirmed. " +
                       $"Total: {order.TotalAmount:C}",
                SentAt = DateTime.Now,
                Attempts = 1
            };

            JsonFileStore.Update<List<EmailMessage>>(DataFiles.GetFullPath(DataFiles.Emails),
                emails =>
                {
                    emails.Add(email);
                    return emails;
                });
        }

        return order.Status == OrderStatus.Completed;
    }

    private string FindInsufficientProductName(Order order)
    {
        foreach (var item in order.Items)
        {
            Product? product = _products.GetById(item.ProductId);

            if (product is null ||
                product.StockQuantity < item.Quantity)
            {
                return product?.Name ?? $"Product #{item.ProductId}";
            }
        }

        return "requested product";
    }
}