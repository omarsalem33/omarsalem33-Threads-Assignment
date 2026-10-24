namespace _06_OrderProcessing.Models;

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
}
//ProductId:int, Quantity:int, UnitPrice:decimal