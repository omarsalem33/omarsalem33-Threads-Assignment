using _06_OrderProcessing.Models;

namespace _06_OrderProcessing.Repository;

public class SyncCustomerRepository
{
    public Customer? GetById(int id)
    {
        const string source = nameof(SyncCustomerRepository);

        Log.Info(source, "Opening connection...");
        Thread.Sleep(200);

        int queryDelay = Random.Shared.Next(300, 601);
        Log.Info(source, $"Executing query for Customer #{id}...");
        Thread.Sleep(queryDelay);

        string filePath = DataFiles.GetFullPath(DataFiles.Customers);
        List<Customer> customers = JsonFileStore.Read<List<Customer>>(filePath);
        Customer? customer = customers.FirstOrDefault(item => item.Id == id);

        if (customer is null)
        {
            Log.Error(source, $"Customer #{id} not found");
            return null;
        }

        Log.Success(source, $"Customer #{id} found ({customer.Name})");
        return customer;
    }
}
