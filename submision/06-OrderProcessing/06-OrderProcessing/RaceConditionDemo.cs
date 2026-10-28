using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;
using _06_OrderProcessing.Service;

namespace _06_OrderProcessing;

public static class RaceConditionDemo
{
    public static async Task RunUnsafeAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);
        
        var productRepository = new ProductRepository();
        var incomingOrderRepository = new IncomingOrderRepository();
        var inventory = new InventoryService(productRepository);

        List<Order> orders = await incomingOrderRepository.GetAllAsync(ct);

        Order order10 = orders.Single(order => order.Id == 10);
        Order order11 = orders.Single(order => order.Id == 10);

        OrderItem item10 = order10.Items.Single(item => item.ProductId == 4);
        OrderItem item11 = order11.Items.Single(item => item.ProductId == 4);

        Task<bool> reservation10 = inventory.ReserveStockUnsafeAsync(item10.ProductId, item10.Quantity, ct);
        Task<bool> reservation11 = inventory.ReserveStockUnsafeAsync(item11.ProductId, item11.Quantity, ct);
        
        bool order10Reserved = await reservation10;
        bool order11Reserved = await reservation11;

        Product? item = await productRepository.GetByIdAsync(4, ct);
        
        Console.WriteLine($"Order 10 reserved: {order10Reserved}");
        Console.WriteLine($"Order 11 reserved: {order11Reserved}");
        Console.WriteLine($"Keyboard stock left: {item?.StockQuantity}");
    }

    public static async Task RunSafeAsync(CancellationToken ct)
    {
        await DataSeeder.ResetAsync(ct);
        
        var productRepository = new ProductRepository();
        var incomingOrderRepository = new IncomingOrderRepository();
        var inventory = new InventoryService(productRepository);

        List<Order> orders = await incomingOrderRepository.GetAllAsync(ct);

        Order order10 = orders.Single(order => order.Id == 10);
        Order order11 = orders.Single(order => order.Id == 10);

        OrderItem item10 = order10.Items.Single(item => item.ProductId == 4);
        OrderItem item11 = order11.Items.Single(item => item.ProductId == 4);

        Task<bool> reservation10 = inventory.ReserveStockSafeAsync(item10.ProductId, item10.Quantity, ct);
        Task<bool> reservation11 = inventory.ReserveStockSafeAsync(item11.ProductId, item11.Quantity, ct);
        
        bool order10Reserved = await reservation10;
        bool order11Reserved = await reservation11;

        Product? item = await productRepository.GetByIdAsync(4, ct);
        
        Console.WriteLine($"Order 10 reserved: {order10Reserved}");
        Console.WriteLine($"Order 11 reserved: {order11Reserved}");
        Console.WriteLine($"Keyboard stock left: {item?.StockQuantity}");
    }
    
}