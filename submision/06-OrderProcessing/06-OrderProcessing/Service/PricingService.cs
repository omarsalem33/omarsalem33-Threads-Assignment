using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing.Service;

public class PricingService
{
    private readonly ProductRepository _productRepository;

    private readonly CustomerRepository _customerRepository;
    private readonly CouponRepository _couponRepository;
    private readonly IEnumerable<int> _failShippingForOrderIds;

    public PricingService(ProductRepository  productRepository,CustomerRepository customerRepository,CouponRepository couponRepository, IEnumerable<int> failShippingForOrderIds)
    {
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _couponRepository = couponRepository;
        _failShippingForOrderIds = failShippingForOrderIds.ToHashSet();
    }

    public async Task<decimal> CalculateSubTotalAsync(Order order, CancellationToken ct)
    {
        decimal subTotal = 0;

        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct );

            if (product == null)
                throw new InvalidOperationException(
                    $"Product {item.ProductId} was not found.");
            item.UnitPrice = product.Price;
            subTotal += item.UnitPrice *  item.Quantity;
        }
        return Math.Round(subTotal, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<decimal> GetDiscountAsync(Order order, CancellationToken ct)
    {
        const string source = nameof(PricingService);

        Log.Info(source, "Discount API: DNS/handshake...");
        await Task.Delay(150, ct);

        Log.Info(source, "Discount API: Sending request...");
        await Task.Delay(300, ct);

        int responseDelay = Random.Shared.Next(500, 1501);
        Log.Info(source, "Discount API: Waiting for response...");
        await Task.Delay(responseDelay, ct);

        var customer = await _customerRepository.GetByIdAsync(order.CustomerId, ct);

        if (customer is null)
            throw new InvalidOperationException($"Customer {order.CustomerId} was not found.");

        decimal discountPercent = customer.LoyaltyTier switch
        {
            LoyaltyTier.Bronze => 0,
            LoyaltyTier.Silver => 5,
            LoyaltyTier.Gold => 10,
            _ => 0
        };

        if (!string.IsNullOrWhiteSpace(order.CouponCode))
        {
            var coupon = await _couponRepository.GetByCodeAsync(order.CouponCode, ct);

            if (coupon is null)
                throw new InvalidOperationException($"Coupon '{order.CouponCode}' was not found.");

            discountPercent += coupon.DiscountPercent;
        }

        discountPercent = Math.Min(discountPercent, 30);

        decimal discount = order.SubTotal * discountPercent / 100;

        return Math.Round(discount, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<decimal> CalculateShippingCostAsync(Order order, CancellationToken ct)
    {
        const string source = nameof(PricingService);

        Log.Info(source, "Shipping API: DNS/handshake...");
        await Task.Delay(150, ct);

        if (_failShippingForOrderIds.Contains(order.Id))
        {
            Log.Error(source, "Shipping provider unavailable");

            throw new ShippingProviderUnavailableException(
                "Shipping provider unavailable");
        }

        Log.Info(source, "Shipping API: Sending request...");
        await Task.Delay(300, ct);

        int responseDelay = Random.Shared.Next(500, 1501);
        Log.Info(source, "Shipping API: Waiting for response...");
        await Task.Delay(responseDelay, ct);

        if (order.SubTotal >= 1000)
        {
            return 0m;
        }

        int totalQuantity = 0;
        decimal totalWeight = 0m;

        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct);

            if (product is null)
            {
                throw new InvalidOperationException(
                    $"Product {item.ProductId} was not found.");
            }

            totalQuantity += item.Quantity;
            totalWeight += product.WeightKg * item.Quantity;
        }

        decimal shippingCost = 5m + (1.50m * totalQuantity) + (2.00m * totalWeight);

        return Math.Round(shippingCost, 2, MidpointRounding.AwayFromZero);
    }
    
    public async  Task<decimal> CalculateTotalAsync(Order order, CancellationToken ct)
        => (await CalculateSubTotalAsync(order, ct) + await CalculateShippingCostAsync(order, ct) ) - await GetDiscountAsync(order, ct);
}
