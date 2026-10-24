using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class OrderValidator
{
    private readonly CustomerRepository _customers;
    private readonly ProductRepository _products;
    private readonly CouponRepository _coupons;
    private readonly OrderRepository _orders;

    public OrderValidator(CustomerRepository customers, ProductRepository products, CouponRepository coupons, OrderRepository orders)
    {
        _customers = customers;
        _products = products;
        _coupons = coupons;
        _orders = orders;
    }

    public async Task ValidateOrderAsync(Order order, CancellationToken ct)
    {
        const string source = nameof(OrderValidator);

        Log.Info(source, $"Validating Order #{order.Id}...");

        Customer? customer = await _customers.GetByIdAsync(order.CustomerId, ct);
        if (customer is null)
        {
            await RejectAsync(order, $"Customer #{order.CustomerId} not found", ct);
            return;
        }

        if (!customer.IsActive)
        {
            await RejectAsync(order, "Customer is inactive", ct);
            return;
        }

        foreach (OrderItem item in order.Items)
        {
            Product? product = await _products.GetByIdAsync(item.ProductId, ct);
            if (product is null)
            {
                await RejectAsync(order, $"Product #{item.ProductId} not found", ct);
                return;
            }

            if (product.StockQuantity < item.Quantity)
            {
                await RejectAsync(order, $"Insufficient stock for '{product.Name}'", ct);
                return;
            }
        }

        if (order.CouponCode is not null)
        {
            Coupon? coupon = await _coupons.GetByCodeAsync(order.CouponCode, ct);
            if (coupon is null)
            {
                await RejectAsync(order, $"Coupon '{order.CouponCode}' does not exist", ct);
                return;
            }

            if (coupon.ExpiryDate < DateTime.Today)
            {
                await RejectAsync(order, $"Coupon '{order.CouponCode}' has expired", ct);
                return;
            }
        }

        order.Status = OrderStatus.Validated;
        order.FailureReason = null;
        await _orders.SaveAsync(order, ct);

        Log.Success(source, $"Order #{order.Id} validated");
    }

    private async Task RejectAsync(Order order, string reason, CancellationToken ct)
    {
        order.Status = OrderStatus.Rejected;
        order.FailureReason = reason;

        Log.Error(nameof(OrderValidator), $"Order #{order.Id} rejected: {reason}");
        await _orders.SaveAsync(order, ct);
    }
}
