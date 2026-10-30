namespace _06_OrderProcessing;

public class InvalidCouponException:System.Exception 
{
    public InvalidCouponException(string message) : base(message)
    {
    }
}