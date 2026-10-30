using _06_OrderProcessing.Models;
using _06_OrderProcessing.Repository;

namespace _06_OrderProcessing.Service;

public class CustomerService
{
    public async Task<Customer> GetCustomerAsync(int id, CancellationToken ct)
    {
        const string source = nameof(CustomerRepository);
        Log.Info(source,"Opening Connection");
        await Task.Delay(200, ct);
        
        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Customer #{id}...");
        await Task.Delay(queryDelay, ct);

        string filePath = DataFiles.GetFullPath(DataFiles.Customers);
        List<Customer> customers = await JsonFileStore.ReadAsync<List<Customer>>(filePath,ct);
        
        Customer? customer = customers.FirstOrDefault(c => c.Id == id);

        if (customer is null)
            throw new CustomerNotFoundException($"Customer #{id} not found");

        Log.Success(source , $"Customer #{id} has been found");
        return customer;
        
    }
}