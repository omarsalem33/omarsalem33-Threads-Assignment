using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class IncomingOrderRepository
{
    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {
        const string source = nameof(IncomingOrderRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 600);
        Log.Info(source, "Executing query for incoming orders...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.IncomingOrders);
        List<Order> orders = await JsonFileStore.ReadAsync<List<Order>>(filePath, ct);

        Log.Success(source, $"Loaded {orders.Count} incoming orders");
        return orders;
    }
}
