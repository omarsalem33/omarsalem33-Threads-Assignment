using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class ProductRepository
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string source = nameof(ProductRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Product #{id}...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Products);
        List<Product> products = await JsonFileStore.ReadAsync<List<Product>>(filePath, ct);
        Product? product = products.FirstOrDefault(item => item.Id == id);

        if (product is null)
        {
            Log.Error(source, $"Product #{id} not found");
            return null;
        }

        Log.Success(source, $"Product #{id} found ({product.Name})");
        return product;
    }

    public async Task<bool> ReserveStockAsync(int productId, int qty, CancellationToken ct)
    {
        const string source = nameof(ProductRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        Log.Info(source, "Beginning transaction...");
        await Task.Delay(100, ct);

        Log.Info(source, $"Updating stock for Product #{productId}...");
        await Task.Delay(400, ct);

        bool reserved = false;
        string filePath = DataFiles.GetFullPath(DataFiles.Products);
        await JsonFileStore.UpdateAsync<List<Product>>(filePath, products =>
        {
            Product? product = products.FirstOrDefault(item => item.Id == productId);
            if (product is null || product.StockQuantity < qty)
            {
                return products;
            }

            product.StockQuantity -= qty;
            reserved = true;
            return products;
        }, ct);

        Log.Info(source, "Committing transaction...");
        await Task.Delay(200, ct);

        if (!reserved)
        {
            Log.Error(source, $"Could not reserve {qty} units for Product #{productId}");
            return false;
        }

        Log.Success(source, $"Reserved {qty} units for Product #{productId}");
        return true;
    }
}
