using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class SyncCouponRepository
{
    public Coupon? GetByCode(string code)
    {
        const string source = nameof(SyncCouponRepository);

        Log.Info(source, "Opening connection...");
        Thread.Sleep(200);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Coupon '{code}'...");
        Thread.Sleep(queryDelay);

        string filePath = DataFiles.GetFullPath(DataFiles.Coupons);
        List<Coupon> coupons = JsonFileStore.Read<List<Coupon>>(filePath);
        Coupon? coupon = coupons.FirstOrDefault(item =>
            string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));

        if (coupon is null)
        {
            Log.Error(source, $"Coupon '{code}' not found");
            return null;
        }

        Log.Success(source, $"Coupon '{coupon.Code}' found");
        return coupon;
    }
}
