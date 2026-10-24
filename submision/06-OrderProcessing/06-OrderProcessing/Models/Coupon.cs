namespace _06_OrderProcessing.Models;

public class Coupon
{
    public string Code { get; set; }
    public int DiscountPercent { get; set; }
    public DateTime ExpiryDate { get; set; }
}

