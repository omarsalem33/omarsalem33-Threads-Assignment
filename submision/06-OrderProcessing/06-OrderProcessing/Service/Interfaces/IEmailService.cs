using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Service.Interfaces;

public interface IEmailService
{
    Task SendConfirmationAsync(Order order, Customer customer);
}