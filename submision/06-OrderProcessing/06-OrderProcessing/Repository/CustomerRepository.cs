using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class CustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
    {
        const string source = nameof(CustomerRepository);

        Log.Info(source, "Opening connection...");
        await Task.Delay(200, ct);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Customer #{id}...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Customers);
        List<Customer> customers = await JsonFileStore.ReadAsync<List<Customer>>(filePath, ct);
        Customer? customer = customers.FirstOrDefault(c => c.Id == id);

        if (customer is null)
        {
            Log.Error(source, $"Customer #{id} not found");
            return null;
        }

        Log.Success(source, $"Customer #{id} found ({customer.Name})");
        return customer;
    }
}
