using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class CouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken ct)
    {
        const string source = nameof(CouponRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Coupon '{code}'...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Coupons);
        List<Coupon> coupons = await JsonFileStore.ReadAsync<List<Coupon>>(filePath, ct);
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
