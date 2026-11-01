using _06_OrderProcessing;
using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing.Service;

public sealed class ReportGenerator
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;

    public ReportGenerator(OrderRepository orderRepository, ProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<Report> GenerateAsync(CancellationToken ct = default)
    {
        List<Order> orders = await _orderRepository.GetAllAsync(ct);

        List<Order> completedOrders = orders
            .Where(order => order.Status == OrderStatus.Completed)
            .ToList();

        decimal totalRevenue = Math.Round(completedOrders.Sum(order => order.TotalAmount), 2, MidpointRounding.AwayFromZero);

        int completedCount = completedOrders.Count;

        decimal averageOrderValue = completedCount == 0 ? 0m : Math.Round(totalRevenue / completedCount, 2, MidpointRounding.AwayFromZero);

        var topSelling = completedOrders
            .SelectMany(order => order.Items)
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .OrderByDescending(product => product.Quantity)
            .ThenBy(product => product.ProductId)
            .FirstOrDefault();

        string topSellingProduct = string.Empty;
        int topSellingQuantity = 0;

        if (topSelling is not null)
        {
            Product? product = await _productRepository.GetByIdAsync(
                topSelling.ProductId,
                ct);

            topSellingProduct = product?.Name ?? throw new InvalidOperationException($"Product #{topSelling.ProductId} was not found.");

            topSellingQuantity = topSelling.Quantity;
        }

        DateTime generatedAt = DateTime.Now;

        var report = new Report
        {
            TotalOrders = orders.Count,
            TotalRevenue = totalRevenue,
            CompletedCount = completedCount,
            RejectedCount = orders.Count(order => order.Status == OrderStatus.Rejected),
            FailedCount = orders.Count(order => order.Status == OrderStatus.Failed),
            PaymentFailedCount = orders.Count(order => order.Status == OrderStatus.PaymentFailed),
            AverageOrderValue = averageOrderValue,
            TopSellingProduct = topSellingProduct,
            TopSellingQuantity = topSellingQuantity,
            GeneratedAt = generatedAt
        };

        DataFiles.EnsureDirectoriesExist();

        string reportPath = DataFiles.GetReportPath(generatedAt);
        await JsonFileStore.WriteAsync(reportPath, report, ct);

        return report;
    }
}