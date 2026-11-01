namespace _06_OrderProcessing;

public static class Log
{
    private static readonly Lock _lock  = new ();

    private static void Write(string source, string message, ConsoleColor color)
    {
        lock (_lock)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(
                    $"[{DateTime.Now:HH:mm:ss.fff}] " +
                    $"[Thread {Environment.CurrentManagedThreadId}] " +
                    $"{source} -> {message}"
                );
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }
    }
    
    public static void Info(string source, string message)=>
        Write(source, message, ConsoleColor.Gray);
    public static void Error(string source, string message)=>
        Write(source, message, ConsoleColor.Red);
    public static void Success(string source, string message)=>
        Write(source, message, ConsoleColor.Green);
    
}