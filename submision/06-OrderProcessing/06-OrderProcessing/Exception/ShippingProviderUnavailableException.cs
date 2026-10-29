namespace _06_OrderProcessing;

public sealed class ShippingProviderUnavailableException :Exception
{
    public ShippingProviderUnavailableException(string message): base(message){}
}
