using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class CouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code,  CancellationToken ct)
    {
        const string source = nameof(CouponRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 600);
        Log.Info(source, $"Executing query for Coupon #{code}...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Coupons);
        List<Coupon> Coupons = await JsonFileStore.ReadAsync<List<Coupon>>(filePath, ct);
        Coupon? coupon = Coupons.FirstOrDefault(c =>
            string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

        if (coupon is null)
        {
            Log.Error(source, $"Customer #{code} not found");
            return null;
        }

        Log.Success(source, $"Customer #{code} found ({coupon.Code})");
        return coupon;
    }
}