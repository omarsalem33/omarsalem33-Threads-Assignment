using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class OrderRepository
{
    private readonly HashSet<int> _failSaveForOrderIds;
    private readonly bool _failuresEnabled;

    public OrderRepository(IEnumerable<int>? failSaveForOrderIds = null, bool failuresEnabled = true)
    {
        _failSaveForOrderIds = failSaveForOrderIds?.ToHashSet() ?? [];
        _failuresEnabled = failuresEnabled;
    }
    
    public async Task SaveAsync(Order order, CancellationToken ct)
    {
        const string source = nameof(OrderRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);
        Log.Info(source, "Beginning transaction...");
        await Task.Delay(100, ct);
        Log.Info(source, $"Inserting or updating Order #{order.Id}...");
        await Task.Delay(400, ct);
        
        if (_failuresEnabled &&
            order.Status == OrderStatus.Completed &&
            _failSaveForOrderIds.Contains(order.Id))
        {
            throw new DatabaseWriteException("Connection lost during commit");
        }

        string filePath = DataFiles.GetFullPath(DataFiles.Orders);
        await JsonFileStore.UpdateAsync<List<Order>>(filePath, orders =>
        {
            int index = orders.FindIndex(existingOrder => existingOrder.Id == order.Id);
            if (index >= 0)
            {
                orders[index] = order;
            }
            else
            {
                orders.Add(order);
            }

            return orders;
        }, ct);

        Log.Info(source, "Committing transaction...");
        await Task.Delay(200, ct);
        Log.Success(source, $"Order #{order.Id} saved");
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {
        const string source = nameof(OrderRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);
        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, "Executing query for all orders...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Orders);
        List<Order> orders = await JsonFileStore.ReadAsync<List<Order>>(filePath, ct);
        Log.Success(source, $"Loaded {orders.Count} orders");
        return orders;
    }

    public async Task<List<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct)
    {
        const string source = nameof(OrderRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);
        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for orders with status {status}...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Orders);
        List<Order> orders = await JsonFileStore.ReadAsync<List<Order>>(filePath, ct);
        List<Order> matchingOrders = orders.Where(order => order.Status == status).ToList();
        Log.Success(source, $"Found {matchingOrders.Count} orders with status {status}");
        return matchingOrders;
    }
}
