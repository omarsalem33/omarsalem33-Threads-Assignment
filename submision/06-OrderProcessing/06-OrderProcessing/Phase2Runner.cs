using System.Diagnostics;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;
using _06_OrderProcessing.Service;

namespace _06_OrderProcessing;

public static class Phase2Runner
{
    public static async Task RunPricingComparisonAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);

        var productRepository = new ProductRepository();
        var customerRepository = new CustomerRepository();
        var couponRepository = new CouponRepository();
        var incomingOrderRepository = new IncomingOrderRepository();

        var pricingService = new PricingService(productRepository, customerRepository, couponRepository, new HashSet<int> { 7 });

        var orderPricer = new OrderPricer(pricingService);

        Order sequentialOrder = (await incomingOrderRepository.GetAllAsync(ct))
            .Single(order => order.Id == 1);

        Order concurrentOrder = (await incomingOrderRepository.GetAllAsync(ct))
            .Single(order => order.Id == 1);

        var sequentialWatch = Stopwatch.StartNew();
        await orderPricer.PriceOrderSequentialAsync(sequentialOrder, ct);
        sequentialWatch.Stop();

        Console.WriteLine($"Sequential pricing: {sequentialWatch.Elapsed.TotalSeconds:F2} s");
        PrintOrderTotals(sequentialOrder);

        var concurrentWatch = Stopwatch.StartNew();
        await orderPricer.PriceOrderAsync(concurrentOrder, ct);
        concurrentWatch.Stop();

        Console.WriteLine($"Concurrent pricing: {concurrentWatch.Elapsed.TotalSeconds:F2} s");
        PrintOrderTotals(concurrentOrder);
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
