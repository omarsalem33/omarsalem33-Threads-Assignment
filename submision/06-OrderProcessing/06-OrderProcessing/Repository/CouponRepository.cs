using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class CouponRepository
{
    public async Task<Coupon?> GetByCodeAsync(string code,  CancellationToken ct)
    {
        const string source = nameof(CouponRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Coupon #{code}...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Coupons);
        List<Coupon> Coupons = await JsonFileStore.ReadAsync<List<Coupon>>(filePath, ct);
        Coupon? Coupon = Coupons.FirstOrDefault(c => c.Code == code);

        if (Coupon is null)
        {
            Log.Error(source, $"Customer #{code} not found");
            return null;
        }

        Log.Success(source, $"Customer #{code} found ({Coupon.Code})");
        return Coupon;
    }
}