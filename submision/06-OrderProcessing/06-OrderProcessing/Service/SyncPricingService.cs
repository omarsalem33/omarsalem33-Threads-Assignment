using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing.Service;

public sealed class SyncPricingService
{
    private readonly SyncProductRepository _products;
    private readonly SyncCustomerRepository _customers;
    private readonly SyncCouponRepository _coupons;

    public SyncPricingService(SyncProductRepository products, SyncCustomerRepository customers, SyncCouponRepository coupons)
    {
        _products = products;
        _customers = customers;
        _coupons = coupons;
    }

    public decimal CalculateTotal(Order order)
    {
        order.SubTotal = CalculateSubTotal(order);

        order.Discount = CalculateDiscount(order);
        order.ShippingCost = CalculateShippingCost(order);

        order.TotalAmount = Math.Round(order.SubTotal - order.Discount + order.ShippingCost, 2, MidpointRounding.AwayFromZero);

        return order.TotalAmount;
    }

    private decimal CalculateSubTotal(Order order)
    {
        decimal subTotal = 0;

        foreach (var item in order.Items)
        {
            Product? product = _products.GetById(item.ProductId);

            if (product is null)
            {
                throw new InvalidOperationException($"Product #{item.ProductId} was not found.");
            }

            item.UnitPrice = product.Price;
            subTotal += item.UnitPrice * item.Quantity;
        }

        return Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);
    }

    private decimal CalculateDiscount(Order order)
    {
        Thread.Sleep(150);
        Thread.Sleep(300);
        Thread.Sleep(Random.Shared.Next(500, 1501));

        Customer? customer = _customers.GetById(order.CustomerId);

        if (customer is null)
        {
            throw new InvalidOperationException($"Customer #{order.CustomerId} was not found.");
        }

        decimal discountPercent = customer.LoyaltyTier switch
        {
            LoyaltyTier.Bronze => 0,
            LoyaltyTier.Silver => 5,
            LoyaltyTier.Gold => 10,
            _ => 0
        };

        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            Coupon? coupon = _coupons.GetByCode(order.CouponCode);

            if (coupon is null)
            {
                throw new InvalidCouponException($"Coupon '{order.CouponCode}' was not found.");
            }

            if (coupon.ExpiryDate < DateTime.Today)
            {
                throw new InvalidCouponException($"Coupon '{order.CouponCode}' has expired.");
            }

            discountPercent += coupon.DiscountPercent;
        }

        discountPercent = Math.Min(discountPercent, 30);

        return Math.Round(order.SubTotal * discountPercent / 100, 2, MidpointRounding.AwayFromZero);
    }

    private decimal CalculateShippingCost(Order order)
    {
        Thread.Sleep(150);
        Thread.Sleep(300);
        Thread.Sleep(Random.Shared.Next(500, 1501));

        if (order.SubTotal >= 1000)
        {
            return 0m;
        }

        int totalQuantity = 0;
        decimal totalWeight = 0m;

        foreach (var item in order.Items)
        {
            Product? product = _products.GetById(item.ProductId);

            if (product is null)
            {
                throw new InvalidOperationException($"Product #{item.ProductId} was not found.");
            }

            totalQuantity += item.Quantity;
            totalWeight += product.WeightKg * item.Quantity;
        }

        decimal shippingCost = 5m + (1.50m * totalQuantity) + (2.00m * totalWeight);

        return Math.Round(shippingCost, 2, MidpointRounding.AwayFromZero);
    }
}