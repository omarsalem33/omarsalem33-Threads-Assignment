namespace _06_OrderProcessing;

public sealed class CustomerNotFoundException : System.Exception
{
    public CustomerNotFoundException(string message) : base(message)
    {
    }
}