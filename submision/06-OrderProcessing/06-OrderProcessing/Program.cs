using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;
using _06_OrderProcessing.Service;
using _06_OrderProcessing.Service.Interfaces;

namespace _06_OrderProcessing;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
         // await Phase1Runner.RunAsync(cts.Token);
        // await Phase2Runner.RunPricingComparisonAsync(cts.Token);
        //
        // await Phase2Runner.RunUnsafeAsync(cts.Token);
        // await Phase2Runner.RunSafeAsync(cts.Token);
        
    //      await Phase2Runner.RunAsync(cts.Token);
    //     
    //     var paymentGateway = new PaymentGateway(
    //         timeoutOrderIds: new[] { 5 },
    //         declineOrderIds: new[] { 8 });
    //
    //     var emailService = new EmailService(
    //         failAttempts: new Dictionary<int, int>
    //         {
    //             [3] = 2,
    //             [17] = int.MaxValue
    //         });
    

        await DataSeeder.ResetAsync(cts.Token);

        var incomingOrderRepository = new IncomingOrderRepository();
        List<Order> orders = await incomingOrderRepository.GetAllAsync(cts.Token);

        var productRepository = new ProductRepository();
        var customerRepository = new CustomerRepository();
        var couponRepository = new CouponRepository();
        var orderRepository = new OrderRepository(failSaveForOrderIds: new[] { 13 });

        var customerService = new CustomerService();
        var inventoryService = new InventoryService(productRepository);


        var pricingService = new PricingService(productRepository, customerRepository, couponRepository, Array.Empty<int>());

        IPaymentGateway paymentGateway = new PaymentGateway(timeoutOrderIds: new[] { 5 }, declineOrderIds: new[] { 8 });

        IEmailService emailService = new EmailService(
            failAttempts: new Dictionary<int, int>
            {
                [3] = 2,
                [17] = int.MaxValue
            });

        var processor = new OrderProcessor(customerService, inventoryService, pricingService, paymentGateway, orderRepository, emailService, productRepository);

        var processingTasks = new List<Task<bool>>();

        foreach (Order order in orders)
        {
            processingTasks.Add(
                processor.ProcessOrderAsync(order, cts.Token));
        }

        foreach (Task<bool> task in processingTasks)
        {
            await task;
        }

        List<Order> savedOrders =
            await orderRepository.GetAllAsync(cts.Token);

        Console.WriteLine();
        Console.WriteLine("Order | Status | Total | Reason");
        Console.WriteLine("--------------------------------");

        foreach (Order order in savedOrders.OrderBy(order => order.Id))
        {
            Console.WriteLine(
                $"{order.Id,5} | {order.Status,-12} | " +
                $"{order.TotalAmount,8:F2} | {order.FailureReason ?? "-"}");
        }
    }
}   
