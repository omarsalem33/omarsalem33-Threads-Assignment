using _06_OrderProcessing.Service;

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
        
         await Phase2Runner.RunAsync(cts.Token);
        
        var paymentGateway = new PaymentGateway(
            timeoutOrderIds: new[] { 5 },
            declineOrderIds: new[] { 8 });

        var emailService = new EmailService(
            failAttempts: new Dictionary<int, int>
            {
                [3] = 2,
                [17] = int.MaxValue
            });
    }
}   
