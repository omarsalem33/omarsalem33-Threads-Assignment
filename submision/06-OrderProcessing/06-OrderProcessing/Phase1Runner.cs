using System.Diagnostics;
using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing;

public static class Phase1Runner
{
    public static async Task RunAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);
        string incomingOrdersPath = DataFiles.GetFullPath(DataFiles.IncomingOrders);
        List<Order> syncOrders = JsonFileStore.Read<List<Order>>(incomingOrdersPath);

        var syncValidator = new SyncOrderValidator(
            new SyncCustomerRepository(),
            new SyncProductRepository(),
            new SyncCouponRepository());

        Stopwatch syncWatch = Stopwatch.StartNew();
        foreach (Order order in syncOrders)
        {
            ct.ThrowIfCancellationRequested();
            syncValidator.ValidateOrder(order);
        }
        syncWatch.Stop();

        await DataSeeder.ResetAsync(ct);

        var orderRepository = new OrderRepository();
        var incomingOrderRepository = new IncomingOrderRepository();
        var validator = new OrderValidator(
            new CustomerRepository(),
            new ProductRepository(),
            new CouponRepository());

        List<Order> asyncOrders = await incomingOrderRepository.GetAllAsync(ct);
        Stopwatch asyncWatch = Stopwatch.StartNew();
        foreach (Order order in asyncOrders)
        {
            await validator.ValidateOrderAsync(order, ct);
            await orderRepository.SaveAsync(order, ct);
        }
        asyncWatch.Stop();

        List<Order> allSavedOrders = await orderRepository.GetAllAsync(ct);
        List<Order> validatedOrders = await orderRepository.GetByStatusAsync(OrderStatus.Validated, ct);
        List<Order> rejectedOrders = await orderRepository.GetByStatusAsync(OrderStatus.Rejected, ct);

        Console.WriteLine("===== Validation Summary =====");
        Console.WriteLine($"Total Orders: {allSavedOrders.Count}");
        Console.WriteLine($"Validated: {validatedOrders.Count}");
        Console.WriteLine($"Rejected: {rejectedOrders.Count}");

        foreach (Order order in rejectedOrders.OrderBy(order => order.Id))
        {
            Console.WriteLine($"- Order #{order.Id}: {order.FailureReason}");
        }

        Console.WriteLine($"Sync Time: {syncWatch.Elapsed.TotalSeconds:F1} seconds");
        Console.WriteLine($"Async Time: {asyncWatch.Elapsed.TotalSeconds:F1} seconds");
        Console.WriteLine($"orders.json now contains {allSavedOrders.Count} records.");
    }
}
