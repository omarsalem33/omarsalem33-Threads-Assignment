namespace _06_OrderProcessing;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        await Phase1Runner.RunAsync(cts.Token);
    }
}
