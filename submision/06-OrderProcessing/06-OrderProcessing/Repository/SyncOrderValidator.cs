using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class SyncOrderValidator
{
    private readonly SyncCustomerRepository _customers;
    private readonly SyncProductRepository _products;
    private readonly SyncCouponRepository _coupons;

    public SyncOrderValidator(SyncCustomerRepository customers, SyncProductRepository products, SyncCouponRepository coupons)
    {
        _customers = customers;
        _products = products;
        _coupons = coupons;
    }

    public void ValidateOrder(Order order)
    {
        const string source = nameof(SyncOrderValidator);
        Log.Info(source, $"Validating Order #{order.Id}...");

        Customer? customer = _customers.GetById(order.CustomerId);
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
            Product? product = _products.GetById(item.ProductId);
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
            Coupon? coupon = _coupons.GetByCode(order.CouponCode);
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
        Log.Error(nameof(SyncOrderValidator), $"Order #{order.Id} rejected: {reason}");
    }
}
