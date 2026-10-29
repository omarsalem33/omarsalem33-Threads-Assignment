using System.Diagnostics;
using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;
using _06_OrderProcessing.Service;

namespace _06_OrderProcessing;

public static class Phase2Runner
{
    public static async Task RunAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);
        await RunReservationDemoAsync(useSafeMethod: false, ct);

        await DataSeeder.ResetAsync(ct);
        await RunReservationDemoAsync(useSafeMethod: true, ct);

        await DataSeeder.ResetAsync(ct);
        await ComparePricingAsync(ct);
        await ProcessAllOrdersAsync(ct);
        
    }

    public static async Task RunUnsafeAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);
        await RunReservationDemoAsync(useSafeMethod: false, ct);
    }

    public static async Task RunSafeAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);
        await RunReservationDemoAsync(useSafeMethod: true, ct);
    }

    public static async Task RunPricingComparisonAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);
        await ComparePricingAsync(ct);
    }

    private static async Task RunReservationDemoAsync(
        bool useSafeMethod,
        CancellationToken ct)
    {
        var productRepository = new ProductRepository();
        var incomingOrderRepository = new IncomingOrderRepository();
        var inventoryService = new InventoryService(productRepository);

        List<Order> orders = await incomingOrderRepository.GetAllAsync(ct);

        Order order10 = orders.Single(order => order.Id == 10);
        Order order11 = orders.Single(order => order.Id == 11);

        OrderItem item10 = order10.Items.Single(item => item.ProductId == 4);
        OrderItem item11 = order11.Items.Single(item => item.ProductId == 4);

        Console.WriteLine(
            useSafeMethod
                ? "\nSafe stock reservation demo"
                : "\nUnsafe stock reservation demo");

        Task<bool> reservation10 = useSafeMethod
            ? inventoryService.ReserveStockSafeAsync(
                item10.ProductId, item10.Quantity, ct)
            : inventoryService.ReserveStockUnsafeAsync(
                item10.ProductId, item10.Quantity, ct);

        Task<bool> reservation11 = useSafeMethod
            ? inventoryService.ReserveStockSafeAsync(
                item11.ProductId, item11.Quantity, ct)
            : inventoryService.ReserveStockUnsafeAsync(
                item11.ProductId, item11.Quantity, ct);

        bool order10Reserved = await reservation10;
        bool order11Reserved = await reservation11;

        Product? keyboard = await productRepository.GetByIdAsync(4, ct);

        Console.WriteLine($"Order 10 reserved: {order10Reserved}");
        Console.WriteLine($"Order 11 reserved: {order11Reserved}");
        Console.WriteLine($"Keyboard stock left: {keyboard?.StockQuantity}");
    }

    private static async Task ComparePricingAsync(CancellationToken ct)
    {
        var productRepository = new ProductRepository();
        var customerRepository = new CustomerRepository();
        var couponRepository = new CouponRepository();
        var incomingOrderRepository = new IncomingOrderRepository();

        var pricingService = new PricingService(
            productRepository,
            customerRepository,
            couponRepository,
            new HashSet<int> { 7 });

        var orderPricer = new OrderPricer(pricingService);

        List<Order> orders = await incomingOrderRepository.GetAllAsync(ct);

        Order sequentialOrder = orders
            .Single(order => order.Id == 1);

        Order concurrentOrder = (await incomingOrderRepository.GetAllAsync(ct))
            .Single(order => order.Id == 1);

        var sequentialWatch = Stopwatch.StartNew();
        await orderPricer.PriceOrderSequentialAsync(sequentialOrder, ct);
        sequentialWatch.Stop();

        Console.WriteLine(
            $"Sequential pricing: {sequentialWatch.Elapsed.TotalSeconds:F2} s");
        PrintOrderTotals(sequentialOrder);

        var concurrentWatch = Stopwatch.StartNew();
        await orderPricer.PriceOrderAsync(concurrentOrder, ct);
        concurrentWatch.Stop();

        Console.WriteLine(
            $"Concurrent pricing: {concurrentWatch.Elapsed.TotalSeconds:F2} s");
        PrintOrderTotals(concurrentOrder);
    }

    private static async Task ProcessAllOrdersAsync(CancellationToken ct)
    {
        var productRepository = new ProductRepository();
        var customerRepository = new CustomerRepository();
        var couponRepository = new CouponRepository();
        var incomingOrderRepository = new IncomingOrderRepository();
        var orderRepository = new OrderRepository();

        var orderValidator = new OrderValidator(
            customerRepository,
            productRepository,
            couponRepository);

        var inventoryService = new InventoryService(productRepository);

        var pricingService = new PricingService(
            productRepository,
            customerRepository,
            couponRepository,
            new HashSet<int> { 7 });

        var orderPricer = new OrderPricer(pricingService);
        var invoiceGenerator = new InvoiceGenerator();

        var orderProcessor = new OrderProcessorV2(inventoryService, productRepository, orderPricer, invoiceGenerator, orderRepository);

        List<Order> incomingOrders = await incomingOrderRepository.GetAllAsync(ct);

        var stopwatch = Stopwatch.StartNew();

        var validationTasks = new List<Task>();

        foreach (Order order in incomingOrders)
        {
            validationTasks.Add(ValidateAndSaveAsync(order, orderValidator, orderRepository, ct));
        }

        foreach (Task validationTask in validationTasks)
        {
            await validationTask;
        }

        List<Order> validatedOrders = incomingOrders
            .Where(order => order.Status == OrderStatus.Validated)
            .ToList();

        var processingTasks = new List<Task<bool>>();
        var processingOrders = new List<Order>();

        foreach (Order order in validatedOrders)
        {
            processingOrders.Add(order);
            processingTasks.Add(
                orderProcessor.ProcessValidatedOrderAsync(order, ct));

            if (processingTasks.Count == 1)
            {
                Console.WriteLine(
                    $"First processing task status: {processingTasks[0].Status}");
            }
        }

        await Task.Delay(1000, ct);

        Console.WriteLine("\nTask statuses after 1 second:");

        for (int i = 0; i < processingTasks.Count; i++)
        {
            Console.WriteLine(
                $"Order #{processingOrders[i].Id}: {processingTasks[i].Status}");
        }

        int taskIndex = 0;

        foreach (Task<bool> task in processingTasks)
        {
            Order order = processingOrders[taskIndex++];

            try
            {
                bool completed = await task;

                Console.WriteLine(
                    $"Order #{order.Id}: returned {completed}, task status {task.Status}");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Order #{order.Id}: task status {task.Status}; {ex.Message}");

                order.Status = OrderStatus.Failed;
                order.FailureReason = ex.Message;

                foreach (OrderItem item in order.Items)
                {
                    await inventoryService.ReleaseStockAsync(
                        item.ProductId,
                        item.Quantity,
                        ct);
                }

                await orderRepository.SaveAsync(order, ct);
            }
        }

        stopwatch.Stop();

        Console.WriteLine("\nFinal task statuses:");

        for (int i = 0; i < processingTasks.Count; i++)
        {
            Console.WriteLine(
                $"Order #{processingOrders[i].Id}: {processingTasks[i].Status}");
        }

        Console.WriteLine(
            $"Order processing finished in {stopwatch.Elapsed.TotalSeconds:F2} s");

        List<Order> savedOrders = await orderRepository.GetAllAsync(ct);

        Console.WriteLine($"orders.json records: {savedOrders.Count}");

        if (savedOrders.Count != 20)
        {
            throw new InvalidOperationException(
                $"Expected 20 saved orders, found {savedOrders.Count}.");
        }

        Console.WriteLine(
            $"Completed: {savedOrders.Count(order => order.Status == OrderStatus.Completed)}, " +
            $"Rejected: {savedOrders.Count(order => order.Status == OrderStatus.Rejected)}, " +
            $"Failed: {savedOrders.Count(order => order.Status == OrderStatus.Failed)}");
        
        var reportGenerator = new ReportGenerator(orderRepository, productRepository);

        Task<Report> reportTask = reportGenerator.GenerateAsync(ct);

        Console.WriteLine("Generating report...");

        Report report = await reportTask;

        Console.WriteLine($"Total orders: {report.TotalOrders}");
        Console.WriteLine($"Completed: {report.CompletedCount}");
        Console.WriteLine($"Rejected: {report.RejectedCount}");
        Console.WriteLine($"Failed: {report.FailedCount}");
        Console.WriteLine($"Payment failed: {report.PaymentFailedCount}");
        Console.WriteLine($"Total revenue: {report.TotalRevenue:F2}");
        Console.WriteLine($"Average order value: {report.AverageOrderValue:F2}");
        Console.WriteLine(
            $"Top selling product: {report.TopSellingProduct} " +
            $"({report.TopSellingQuantity} units)");
    }

    private static async Task ValidateAndSaveAsync(
        Order order,
        OrderValidator validator,
        OrderRepository orderRepository,
        CancellationToken ct)
    {
        await validator.ValidateOrderAsync(order, ct);
        await orderRepository.SaveAsync(order, ct);
    }

    private static void PrintOrderTotals(Order order)
    {
        Console.WriteLine(
            $"Order #{order.Id}: " +
            $"SubTotal={order.SubTotal:F2}, " +
            $"Discount={order.Discount:F2}, " +
            $"Shipping={order.ShippingCost:F2}, " +
            $"Total={order.TotalAmount:F2}");
    }
    
    
}