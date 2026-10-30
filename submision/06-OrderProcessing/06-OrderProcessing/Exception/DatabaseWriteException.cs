namespace _06_OrderProcessing;

public sealed class DatabaseWriteException : System.Exception
{
    public DatabaseWriteException(string message)
        : base(message)
    {
    }
}