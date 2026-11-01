using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing.Service;

public class InventoryService
{
    private readonly ProductRepository _productRepository;

    public InventoryService(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> ReserveStockUnsafeAsync(int productId, int qty, CancellationToken ct)
    {
        const string source = nameof(ProductRepository);
        string filePath = DataFiles.GetFullPath(DataFiles.Products);

        List<Product> products =  await JsonFileStore.ReadAsync<List<Product>>(filePath, ct);

        Product? product = products.FirstOrDefault(p => p.Id == productId);

        if (product is null || product.StockQuantity < qty)
            return false;

        int stockRead = product.StockQuantity;

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(source, "Beginning transaction...");
        await Task.Delay(100, ct);

        Log.Info(source, $"Updating stock for Product #{productId}...");
        await Task.Delay(400, ct);

        product.StockQuantity = stockRead - qty;

        await JsonFileStore.WriteAsync(filePath, products, ct);

        Log.Success(source, $"Reserved {qty} units for Product #{productId}");
        return true;
    }

    public async Task<bool> ReserveStockSafeAsync(int productId, int qty, CancellationToken ct)
     => await _productRepository.ReserveStockAsync(productId, qty, ct);

    public async Task ReleaseStockAsync(int productId, int qty, CancellationToken ct)
        => await _productRepository.ReleaseStockAsync(productId, qty, ct);
    
    public async Task<bool> ReserveStockAsync(Order order, CancellationToken ct) 
    { 
        const string source = nameof(InventoryService);
        string path = DataFiles.GetFullPath(DataFiles.Products);

        var requestedItems = order.Items.GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            }).ToList();

        await Task.Delay(200, ct);

        bool reserved = false;

        await JsonFileStore.UpdateAsync<List<Product>>(path, products =>
            {
                foreach (var item in requestedItems)
                {
                    Product? product = products.FirstOrDefault(p => p.Id == item.ProductId);

                    if (product is null || product.StockQuantity < item.Quantity)
                        return products;
                }

                foreach (var item in requestedItems)
                {
                    Product product = products.First(p => p.Id == item.ProductId);
                    product.StockQuantity -= item.Quantity;
                }

                reserved = true;
                return products;
            }, ct);

        if (!reserved)
        {
            Log.Error(source, $"Could not reserve stock for Order #{order.Id}");
            return false;
        }

        Log.Success(source, $"Reserved stock for Order #{order.Id}");
        return true;
    }

public async Task ReleaseStockAsync(Order order, CancellationToken ct)
    {
        string path = DataFiles.GetFullPath(DataFiles.Products);

        var requestedItems = order.Items.GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            }).ToList();

        await JsonFileStore.UpdateAsync<List<Product>>(path, products => 
            {
                foreach (var item in requestedItems)
                {
                    bool productExists = products.Any(p => p.Id == item.ProductId);

                    if (!productExists)
                    {
                        throw new InvalidOperationException($"Product #{item.ProductId} was not found.");
                    }
                }

                foreach (var item in requestedItems)
                {
                    Product product = products.First(p => p.Id == item.ProductId);
                    product.StockQuantity += item.Quantity;
                }

                return products;
            }, ct);

        Log.Success(source: nameof(InventoryService), message: $"Released stock for Order #{order.Id}");
    }
}