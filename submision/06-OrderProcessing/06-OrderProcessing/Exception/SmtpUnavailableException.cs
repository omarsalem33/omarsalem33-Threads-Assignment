namespace _06_OrderProcessing;

public class SmtpUnavailableException :System.Exception
{
    public SmtpUnavailableException(string message)
        : base(message)
    {
    }
}