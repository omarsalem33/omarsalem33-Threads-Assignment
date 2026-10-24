using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing;

class Program
{
    static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        // await DataSeeder.ResetAsync(cts.Token);
        //
        // CustomerRepository  customerRepository = new CustomerRepository();
        // await customerRepository.GetByIdAsync(12,  cts.Token);
        // await customerRepository.GetByIdAsync(1,  cts.Token);
        //
        // ProductRepository  productRepository = new ProductRepository();
        // await productRepository.GetByIdAsync(1,  cts.Token);
        // await productRepository.GetByIdAsync(133,  cts.Token);
        // await productRepository.ReserveStockAsync(1,3,  cts.Token);
        //
        // CouponRepository couponRepository = new CouponRepository();
        // await couponRepository.GetByCodeAsync("SAVE10",cts.Token);
        //
        //
        //  OrderRepository  orderRepository = new OrderRepository();
        // // Coupon coupon = new Coupon();
        // Order ordertest = new Order();
        // //
        // // if (coupon.ExpiryDate < DateTime.Today)
        // // {
        // //     order.Status = OrderStatus.Rejected;
        // //     order.FailureReason = $"Coupon '{coupon.Code}' has expired";
        // //     
        // //     await orderRepository.SaveAsync(order, cts.Token);
        // // }
        //
        // if (ordertest.CouponCode is not null)
        // {
        //     Coupon? coupon = await couponRepository.GetByCodeAsync(ordertest.CouponCode, cts.Token);
        //
        //     if (coupon is null)
        //     {
        //         ordertest.Status = OrderStatus.Rejected;
        //         ordertest.FailureReason =
        //             $"Coupon '{ordertest.CouponCode}' does not exist";
        //
        //         await orderRepository.SaveAsync(ordertest, cts.Token);
        //         return;
        //     }
        //
        //     if (coupon.ExpiryDate < DateTime.Today)
        //     {
        //         ordertest.Status = OrderStatus.Rejected;
        //         ordertest.FailureReason =
        //             $"Coupon '{ordertest.CouponCode}' has expired";
        //
        //         await orderRepository.SaveAsync(ordertest, cts.Token);
        //         return;
        //     }
        // }
        //
        // ordertest.Status = OrderStatus.Validated;
        // ordertest.FailureReason = null;
        // await orderRepository.SaveAsync(ordertest, cts.Token);
        //===========================================//

        await DataSeeder.ResetAsync(cts.Token);

        var customers = new CustomerRepository();
        var products = new ProductRepository();
        var coupons = new CouponRepository();
        var orders = new OrderRepository();
        var incomingOrdersRepository = new IncomingOrderRepository();

        var validator = new OrderValidator(customers, products, coupons, orders);

        List<Order> incomingOrders =
            await incomingOrdersRepository.GetAllAsync(cts.Token);

        foreach (Order order in incomingOrders)
        {
            await validator.ValidateOrderAsync(order, cts.Token);
        }
    }

}