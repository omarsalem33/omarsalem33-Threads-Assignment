using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing.Service;

public class OrderProcessorV2
{
    public OrderProcessorV2(InventoryService inventoryService, ProductRepository productRepository, OrderPricer orderPricer, InvoiceGenerator invoiceGenerator, OrderRepository orderRepository)
    {
        _inventoryService = inventoryService;
        _productRepository = productRepository;
        _orderPricer = orderPricer;
        _invoiceGenerator = invoiceGenerator;
        _orderRepository = orderRepository;
    }

    private readonly InventoryService _inventoryService;
    private readonly ProductRepository _productRepository;
    private readonly OrderPricer _orderPricer;
    private readonly InvoiceGenerator _invoiceGenerator;
    private readonly OrderRepository _orderRepository;

    public async Task<bool> ProcessValidatedOrderAsync(Order order, CancellationToken ct)
    {
        var reservedItems = new List<OrderItem>();

        foreach (OrderItem item in order.Items)
        {
            bool reserved = await _inventoryService.ReserveStockSafeAsync(item.ProductId, item.Quantity, ct);

            if (!reserved)
            {
                foreach (OrderItem reservedItem in reservedItems)
                    await _inventoryService.ReleaseStockAsync(reservedItem.ProductId, reservedItem.Quantity, ct);
                

                Product? product = await _productRepository.GetByIdAsync(item.ProductId, ct);

                string productName = product?.Name ?? $"Product #{item.ProductId}";

                order.Status = OrderStatus.Rejected;
                order.FailureReason =
                    $"Insufficient stock for '{productName}'";

                await _orderRepository.SaveAsync(order, ct);
                return false;
            }

            reservedItems.Add(item);
        }

        await _orderPricer.PriceOrderAsync(order, ct);
        await _invoiceGenerator.GenerateAsync(order);

        order.Status = OrderStatus.Completed;
        order.FailureReason = null;

        await _orderRepository.SaveAsync(order, ct);
        return true;
    }
}