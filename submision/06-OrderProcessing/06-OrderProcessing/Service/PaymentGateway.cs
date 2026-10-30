using _06_OrderProcessing.Enums;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Service.Interfaces;

namespace _06_OrderProcessing.Service;

public class PaymentGateway :IPaymentGateway
{
    public PaymentGateway(IEnumerable<int>? timeoutOrderIds = null, IEnumerable<int> declineOrderIds = null, bool failuresEnabled = true)
    {
        _timeoutOrderIds = timeoutOrderIds?.ToHashSet() ?? [];
        _declineOrderIds = declineOrderIds?.ToHashSet() ?? [];
        _failuresEnabled = failuresEnabled;
    }

    private readonly HashSet<int> _timeoutOrderIds;
    private readonly HashSet<int> _declineOrderIds;
    private readonly bool _failuresEnabled;
    
    public async Task<PaymentResult> ProcessPaymentAsync(int orderId, decimal amount, CancellationToken ct)
    {
      bool shouldTimeOut = _failuresEnabled && _timeoutOrderIds.Contains(orderId);

      await Task.Delay(shouldTimeOut ? 5000 : 700, ct);

      string transactionId = $"{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
      
      bool declined = _failuresEnabled && _declineOrderIds.Contains(orderId);

      var transaction = new PaymentTransaction
      {
          OrderId = orderId,
          Amount = amount,
          TransactionId = transactionId,
          Status = declined ? PaymentStatus.Declined : PaymentStatus.Captured,
          CreatedAt = DateTime.Now
      };

      string path = DataFiles.GetFullPath(DataFiles.Payments);
      
      await JsonFileStore.UpdateAsync<List<PaymentTransaction>> (path, payment =>
      {
          payment.Add(transaction);
          return payment;
      },ct);
      return new PaymentResult
      {
          Success = !declined,
          TransactionId = transactionId,
          Status = transaction.Status,
          Message = declined ? "Card declined by issuer" : null
      };
    }

    public async Task RefundAsync(string transactionId)
    {
        string path = DataFiles.GetFullPath(DataFiles.Payments);
        bool found = false;

        await JsonFileStore.UpdateAsync<List<PaymentTransaction>>(
            path,
            payments =>
            {
                PaymentTransaction? payment = payments.FirstOrDefault(
                    p => p.TransactionId == transactionId);

                if (payment is not null)
                {
                    payment.Status = PaymentStatus.Refunded;
                    found = true;
                }

                return payments;
            },
            CancellationToken.None);

        if (!found)
        {
            throw new InvalidOperationException(
                $"Payment transaction '{transactionId}' was not found.");
        }
    }
}