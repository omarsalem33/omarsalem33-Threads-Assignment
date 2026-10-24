using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class ProductRepository
{
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string source = nameof(ProductRepository);
        
        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 600);
        Log.Info(source , $"Executing query for Customer #{id}...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Products);
        List<Product> products = await JsonFileStore.ReadAsync<List<Product>>(filePath, ct);

        Product? product = products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            Log.Error(source, $"Product #{id} not found");
            return null;
        }

        Log.Success(source, $"Product #{id} found ({product.Name})");
        return product;

    }

    // public async Task<List<Product>> GetAllAsync(CancellationToken ct)
    // {
    //     const string source = nameof(ProductRepository);
    //     Log.Info(source,"Opening connection...");
    //     await Task.Delay(200, ct);
    //     int queryDelay = Random.Shared.Next(300, 600);
    //     
    //     string filePath = DataFiles.GetFullPath(DataFiles.Products);
    //     List<Product>? products = await JsonFileStore.ReadAsync<List<Product>>(filePath, ct);
    //
    //     if (products == null)
    //     {
    //         Log.Error(source,"Products not found");
    //         return null;
    //     }
    //     Log.Success(source, $"Products found ({products.Count})");
    //     return products;
    // }

    public async Task<bool> ReserveStockAsync(int productId, int qty , CancellationToken ct)
    {
        const string source = nameof(ProductRepository);
        
        Log.Info(source,"Opening connection...");

        Product product = await GetByIdAsync(productId, ct);

        if (product.StockQuantity- qty  < 0 )
        {
            Log.Error(source,"Products not found");
            return false;
        }
        Log.Success(source,$"Product StockQuantity is {qty - product.StockQuantity}");
        return true;
    }
}