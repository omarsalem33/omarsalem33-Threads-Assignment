using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class SyncOrderRepository
{
    public void Save(Order order)
    {
        const string source = nameof(SyncOrderRepository);

        Log.Info(source, "Opening connection...");
        Thread.Sleep(200);
        Log.Info(source, "Beginning transaction...");
        Thread.Sleep(100);
        Log.Info(source, $"Inserting or updating Order #{order.Id}...");
        Thread.Sleep(400);

        string filePath = DataFiles.GetFullPath(DataFiles.Orders);
        JsonFileStore.Update<List<Order>>(filePath, orders =>
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
        });

        Log.Info(source, "Committing transaction...");
        Thread.Sleep(200);
        Log.Success(source, $"Order #{order.Id} saved");
    }

    public List<Order> GetAll()
    {
        const string source = nameof(SyncOrderRepository);

        Log.Info(source, "Opening connection...");
        Thread.Sleep(200);
        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, "Executing query for all orders...");
        Thread.Sleep(queryDelay);

        string filePath = DataFiles.GetFullPath(DataFiles.Orders);
        List<Order> orders = JsonFileStore.Read<List<Order>>(filePath);
        Log.Success(source, $"Loaded {orders.Count} orders");
        return orders;
    }

    public List<Order> GetByStatus(OrderStatus status)
    {
        const string source = nameof(SyncOrderRepository);

        Log.Info(source, "Opening connection...");
        Thread.Sleep(200);
        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for orders with status {status}...");
        Thread.Sleep(queryDelay);

        string filePath = DataFiles.GetFullPath(DataFiles.Orders);
        List<Order> orders = JsonFileStore.Read<List<Order>>(filePath);
        List<Order> matchingOrders = orders.Where(order => order.Status == status).ToList();
        Log.Success(source, $"Found {matchingOrders.Count} orders with status {status}");
        return matchingOrders;
    }
}
