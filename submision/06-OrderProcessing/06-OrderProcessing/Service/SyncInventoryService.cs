using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Service;

public sealed class SyncInventoryService
{
    public bool ReserveStock(Order order)
    {
        string path = DataFiles.GetFullPath(DataFiles.Products);

        var requestedItems = order.Items.GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            }).ToList();

        Thread.Sleep(200);

        bool reserved = false;

        JsonFileStore.Update<List<Product>>(
            path,
            products =>
            {
                foreach (var item in requestedItems)
                {
                    Product? product = products.FirstOrDefault(p => p.Id == item.ProductId);

                    if (product is null || product.StockQuantity < item.Quantity)
                    {
                        return products;
                    }
                }

                foreach (var item in requestedItems)
                {
                    Product product = products.First(p => p.Id == item.ProductId);

                    product.StockQuantity -= item.Quantity;
                }

                reserved = true;
                return products;
            });

        return reserved;
    }

    public void ReleaseStock(Order order)
    {
        string path = DataFiles.GetFullPath(DataFiles.Products);

        var requestedItems = order.Items.GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            }).ToList();

        JsonFileStore.Update<List<Product>>(path, products =>
            {
                foreach (var item in requestedItems)
                {
                    if (!products.Any(p => p.Id == item.ProductId))
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
            });
    }
}