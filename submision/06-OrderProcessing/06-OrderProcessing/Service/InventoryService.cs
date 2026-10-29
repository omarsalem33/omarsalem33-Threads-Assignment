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
}