using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class SyncProductRepository
{
    public Product? GetById(int id)
    {
        const string source = nameof(SyncProductRepository);

        Log.Info(source, "Opening connection...");
        Thread.Sleep(200);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Product #{id}...");
        Thread.Sleep(queryDelay);

        string filePath = DataFiles.GetFullPath(DataFiles.Products);
        List<Product> products = JsonFileStore.Read<List<Product>>(filePath);
        Product? product = products.FirstOrDefault(item => item.Id == id);

        if (product is null)
        {
            Log.Error(source, $"Product #{id} not found");
            return null;
        }

        Log.Success(source, $"Product #{id} found ({product.Name})");
        return product;
    }

    public bool ReserveStock(int productId, int qty)
    {
        const string source = nameof(SyncProductRepository);

        Log.Info(source, "Opening connection...");
        Thread.Sleep(200);

        Log.Info(source, "Beginning transaction...");
        Thread.Sleep(100);

        Log.Info(source, $"Updating stock for Product #{productId}...");
        Thread.Sleep(400);

        bool reserved = false;
        string filePath = DataFiles.GetFullPath(DataFiles.Products);
        JsonFileStore.Update<List<Product>>(filePath, products =>
        {
            Product? product = products.FirstOrDefault(item => item.Id == productId);
            if (product is null || product.StockQuantity < qty)
            {
                return products;
            }

            product.StockQuantity -= qty;
            reserved = true;
            return products;
        });

        Log.Info(source, "Committing transaction...");
        Thread.Sleep(200);

        if (!reserved)
        {
            Log.Error(source, $"Could not reserve {qty} units for Product #{productId}");
            return false;
        }

        Log.Success(source, $"Reserved {qty} units for Product #{productId}");
        return true;
    }
}
