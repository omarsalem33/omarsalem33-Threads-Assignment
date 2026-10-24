namespace _06_OrderProcessing;

class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        await DataSeeder.ResetAsync(cts.Token);
    }

}