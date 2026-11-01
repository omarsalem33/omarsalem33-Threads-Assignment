using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Service;

public class OrderPricer
{
    private readonly PricingService _pricingService;

    public OrderPricer(PricingService  pricingService )
    {
        _pricingService = pricingService;
    }

    public async Task PriceOrderSequentialAsync(Order order, CancellationToken ct)
    {
        order.SubTotal = await _pricingService.CalculateSubTotalAsync(order, ct);
        order.Discount = await _pricingService.GetDiscountAsync(order, ct);
        order.ShippingCost = await _pricingService.CalculateShippingCostAsync(order, ct);

        CalculateTotal(order);

    }

    public async Task PriceOrderAsync(Order order , CancellationToken ct)
    {
        order.SubTotal = await _pricingService.CalculateSubTotalAsync(order, ct);

        Task<decimal> discountTask = _pricingService.GetDiscountAsync(order, ct);

        Task<decimal> shippingTask = _pricingService.CalculateShippingCostAsync(order, ct);

        order.Discount = await discountTask;
        order.ShippingCost = await shippingTask;

        CalculateTotal(order);
    }


    private static void CalculateTotal(Order order) =>
        order.TotalAmount = Math.Round(order.SubTotal - order.Discount + order.ShippingCost, 2, MidpointRounding.AwayFromZero);

}
