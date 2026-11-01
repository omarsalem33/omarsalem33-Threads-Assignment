using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Service.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount ,CancellationToken ct);
    Task RefundAsync(string transactionId);
}