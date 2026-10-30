namespace _06_OrderProcessing;

public class DataFileCorruptedException : Exception
{
    public string FileName { get; }

    public DataFileCorruptedException(string fileName, string message, Exception? exception = null)
        : base(message, exception) =>
            this.FileName = fileName;
}