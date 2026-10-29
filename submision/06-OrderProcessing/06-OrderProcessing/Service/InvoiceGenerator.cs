using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Service;

public sealed class InvoiceGenerator
{
    public async Task GenerateAsync(Order order)
    {
        long renderChecksum = await Task.Run(() =>
        {
            long checksum = 0;

            for (int i = 0; i < 20_000_000; i++)
            {
                checksum += i * 31L;
            }

            return checksum;
        });

        DataFiles.EnsureDirectoriesExist();

        string path = DataFiles.GetInvoicePath(order.Id.ToString());

        string content =
            $"""
             Invoice #{order.Id}
             -------------------------
             Customer ID: {order.CustomerId}

             {string.Join(Environment.NewLine,
                 order.Items.Select(item =>
                     $"Product #{item.ProductId} | Quantity: {item.Quantity} | Unit price: {item.UnitPrice:F2}"))}

             Subtotal: {order.SubTotal:F2}
             Discount: {order.Discount:F2}
             Shipping: {order.ShippingCost:F2}
             Total:    {order.TotalAmount:F2}
             Status:   {order.Status}
             """;

        Log.Info(nameof(InvoiceGenerator), $"Invoice rendering simulation completed ({renderChecksum}).");

        await File.WriteAllTextAsync(path, content);
    }
}