using System.Collections.Concurrent;
using _06_OrderProcessing.Models;
using _06_OrderProcessing.Service.Interfaces;

namespace _06_OrderProcessing.Service;

public sealed class EmailService : IEmailService
{
    private readonly IReadOnlyDictionary<int, int> _failAttempts;
    private readonly bool _failuresEnabled;
    private readonly int _delayMilliseconds;
    private readonly ConcurrentDictionary<int, int> _attempts = new();

    public EmailService(
        IReadOnlyDictionary<int, int>? failAttempts = null,
        bool failuresEnabled = true,
        int delayMilliseconds = 500)
    {
        _failAttempts = failAttempts ??
            new Dictionary<int, int>();

        _failuresEnabled = failuresEnabled;
        _delayMilliseconds = delayMilliseconds;
    }

    public async Task SendConfirmationAsync(Order order, Customer customer)
    {
        int attempt = _attempts.AddOrUpdate(
            order.Id,
            1,
            (_, previous) => previous + 1);

        await Task.Delay(_delayMilliseconds);

        if (_failuresEnabled &&
            _failAttempts.TryGetValue(order.Id, out int attemptsToFail) &&
            attempt <= attemptsToFail)
        {
            throw new SmtpUnavailableException(
                $"Email for order #{order.Id} failed on attempt {attempt}.");
        }

        var email = new EmailMessage
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            To = customer.Email,
            Subject = $"Order #{order.Id} confirmation",
            Body = $"Your order #{order.Id} is confirmed. Total: {order.TotalAmount:C}",
            SentAt = DateTime.Now,
            Attempts = attempt
        };

        string path = DataFiles.GetFullPath(DataFiles.Emails);

        await JsonFileStore.UpdateAsync<List<EmailMessage>>(
            path,
            emails =>
            {
                emails.Add(email);
                return emails;
            },
            CancellationToken.None);
    }
}