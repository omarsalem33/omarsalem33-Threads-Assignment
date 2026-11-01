using System.Collections.Concurrent;
using System.Diagnostics;
using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;
using _06_OrderProcessing.Service;
using _06_OrderProcessing.Service.Interfaces;

namespace _06_OrderProcessing;

public static class Phase3Runner
{
    
    public static async Task RunComparisonAsync(CancellationToken ct)
    {
        await RunSyncVersionAsync(ct);
        await RunAsyncVersionAsync(ct);
    }

    private static async Task RunSyncVersionAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);

        List<Order> orders = JsonFileStore.Read<List<Order>>(DataFiles.GetFullPath(DataFiles.IncomingOrders));
        orders = orders.Take(10).ToList();
        var syncProducts = new SyncProductRepository();
        var syncCustomers = new SyncCustomerRepository();
        var syncCoupons = new SyncCouponRepository();
        var syncOrderRepository = new SyncOrderRepository();

        var validator = new SyncOrderValidator(syncCustomers, syncProducts, syncCoupons);

        var inventory = new SyncInventoryService();
        var pricing = new SyncPricingService(syncProducts, syncCustomers, syncCoupons);

        var processor = new SyncOrderProcessor(validator, inventory, pricing, syncOrderRepository, syncCustomers, syncProducts);

        var usedThreads = new ConcurrentDictionary<int, byte>();
        var results = new bool[orders.Count];
        var errors = new Exception?[orders.Count];
        var threads = new Thread[orders.Count];

        int poolBefore = ThreadPool.ThreadCount;
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < orders.Count; i++)
        {
            int index = i;

            threads[index] = new Thread(() =>
            {
                usedThreads.TryAdd(
                    Environment.CurrentManagedThreadId, 0);

                try
                {
                    results[index] = processor.ProcessOrder(orders[index]);
                }
                catch (Exception ex)
                {
                    errors[index] = ex;
                }
            });
        }

        foreach (Thread thread in threads)
        {
            thread.Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        stopwatch.Stop();
        int poolAfter = ThreadPool.ThreadCount;

        if (errors.Any(error => error is not null))
        {
            throw new AggregateException(errors.Where(error => error is not null)!);
        }

        List<Order> savedOrders = syncOrderRepository.GetAll();

        PrintSummary("Sync", savedOrders, stopwatch.Elapsed, usedThreads.Count, poolBefore, poolAfter);
    }

    private static async Task RunAsyncVersionAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);

        var incomingOrderRepository = new IncomingOrderRepository();
        List<Order> orders =
            await incomingOrderRepository.GetAllAsync(ct);

        var productRepository = new ProductRepository();
        var customerRepository = new CustomerRepository();
        var couponRepository = new CouponRepository();

        var orderRepository = new OrderRepository(failSaveForOrderIds: new[] { 13 }, failuresEnabled: false);

        var customerService = new CustomerService();
        var inventoryService = new InventoryService(productRepository);

        var pricingService = new PricingService(productRepository, customerRepository, couponRepository,
            Array.Empty<int>());

        IPaymentGateway paymentGateway = new PaymentGateway(
            timeoutOrderIds: new[] { 5 },
            declineOrderIds: new[] { 8 },
            failuresEnabled: false);

        IEmailService emailService = new EmailService(
            failAttempts: new Dictionary<int, int>
            {
                [3] = 2,
                [17] = int.MaxValue
            },
            failuresEnabled: false);

        var usedThreads = new ConcurrentDictionary<int, byte>();

        var processor = new OrderProcessor(
            customerService,
            inventoryService,
            pricingService,
            paymentGateway,
            orderRepository,
            emailService,
            productRepository,
            threadObserved: threadId => usedThreads.TryAdd(threadId, 0));

        int poolBefore = ThreadPool.ThreadCount;
        var stopwatch = Stopwatch.StartNew();

        var tasks = new List<Task<bool>>();

        foreach (Order order in orders.Take(10))
        {
            tasks.Add(processor.ProcessOrderAsync(order, ct));
        }

        foreach (Task<bool> task in tasks)
        {
            await task;
        }

        stopwatch.Stop();
        int poolAfter = ThreadPool.ThreadCount;

        List<Order> savedOrders =
            await orderRepository.GetAllAsync(ct);

        PrintSummary("Async", savedOrders, stopwatch.Elapsed, usedThreads.Count, poolBefore, poolAfter);
    }

    private static void PrintSummary(string version, List<Order> orders, TimeSpan elapsed, int usedThreadCount, int poolBefore, int poolAfter)
    {
        int completed = orders.Count(
            order => order.Status == OrderStatus.Completed);

        int rejected = orders.Count(
            order => order.Status == OrderStatus.Rejected);

        Console.WriteLine();
        Console.WriteLine($"--- {version} results ---");
        Console.WriteLine($"Orders saved: {orders.Count}");
        Console.WriteLine($"Completed: {completed}");
        Console.WriteLine($"Rejected: {rejected}");
        Console.WriteLine($"Other statuses: {orders.Count - completed - rejected}");
        Console.WriteLine($"Elapsed: {elapsed.TotalSeconds:F2} s");
        Console.WriteLine($"Unique threads observed: {usedThreadCount}");
        Console.WriteLine(
            $"ThreadPool.ThreadCount: {poolBefore} -> {poolAfter}");

        bool expectedResults = orders.Count == 10 && completed == 8 && rejected == 2 &&
                               orders.Where(order => order.Status == OrderStatus.Rejected)
                                   .Select(order => order.Id)
                                   .OrderBy(id => id)
                                   .SequenceEqual(new[] { 4, 9 });

        Console.WriteLine($"Expected 8 Completed and #4/#9 Rejected: {expectedResults}");
    }
}