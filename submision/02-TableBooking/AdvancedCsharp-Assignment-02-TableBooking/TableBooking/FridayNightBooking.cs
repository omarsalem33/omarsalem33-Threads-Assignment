namespace TableBooking;

public sealed class FridayNightBooking
{
    public int TablesLeft { get; private set; } = 5; 
    public int Booked { get; private set; }
    
    private readonly Lock _lock = new();
    public void Book(string customerName)
    {
        int tablesLeftAfterBooking;
        lock (_lock)
        {
            if (TablesLeft <= 0)
                return;
            Thread.Sleep(20);

            TablesLeft--;
            Booked++;
            tablesLeftAfterBooking = TablesLeft;
        }

        Console.WriteLine($"{customerName} booked a table. Left={tablesLeftAfterBooking}");
    }
}