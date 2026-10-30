using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class OrderValidator
{
    private readonly CustomerRepository _customers;
    private readonly ProductRepository _products;
    private readonly CouponRepository _coupons;

    public OrderValidator(
        CustomerRepository customers,
        ProductRepository products,
        CouponRepository coupons)
    {
        _customers = customers;
        _products = products;
        _coupons = coupons;
    }

    public async Task ValidateOrderAsync(Order order, CancellationToken ct)
    {
        const string source = nameof(OrderValidator);
        Log.Info(source, $"Validating Order #{order.Id}...");

        Customer? customer = await _customers.GetByIdAsync(order.CustomerId, ct);
        if (customer is null)
        {
            Reject(order, $"Customer #{order.CustomerId} not found");
            return;
        }

        if (!customer.IsActive)
        {
            Reject(order, "Customer is inactive");
            return;
        }

        foreach (OrderItem item in order.Items)
        {
            Product? product = await _products.GetByIdAsync(item.ProductId, ct);
            if (product is null)
            {
                Reject(order, $"Product #{item.ProductId} not found");
                return;
            }

            if (product.StockQuantity < item.Quantity)
            {
                Reject(order, $"Insufficient stock for '{product.Name}'");
                return;
            }
        }

        if (order.CouponCode is not null)
        {
            Coupon? coupon = await _coupons.GetByCodeAsync(order.CouponCode, ct);
            if (coupon is null)
            {
                Reject(order, $"Coupon '{order.CouponCode}' does not exist");
                return;
            }

            if (coupon.ExpiryDate < DateTime.Today)
            {
                Reject(order, $"Coupon '{order.CouponCode}' has expired");
                return;
            }
        }

        order.Status = OrderStatus.Validated;
        order.FailureReason = null;
        Log.Success(source, $"Order #{order.Id} validated");
    }

    private static void Reject(Order order, string reason)
    {
        order.Status = OrderStatus.Rejected;
        order.FailureReason = reason;
        Log.Error(nameof(OrderValidator), $"Order #{order.Id} rejected: {reason}");
    }
}
