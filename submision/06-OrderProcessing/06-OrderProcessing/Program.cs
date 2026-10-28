using _06_OrderProcessing.Service;

namespace _06_OrderProcessing;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        // await Phase1Runner.RunAsync(cts.Token);
        await Phase2Runner.RunPricingComparisonAsync(cts.Token);
    }
}
