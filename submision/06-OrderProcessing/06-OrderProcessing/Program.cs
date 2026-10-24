namespace _06_OrderProcessing;

class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        await DataSeeder.ResetAsync(cts.Token);
        
        // Log.Info("CustomerRepository", "Opening connection...");
        // Log.Success("CustomerRepository", "Customer found");
        // Log.Error("CustomerRepository", "Customer not found");
    }

}