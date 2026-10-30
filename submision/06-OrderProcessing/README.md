# 06 Mini Project - Order Processing System

## Project Overview

This project is a C# console application that processes customer orders using JSON files as storage. It covers synchronous and asynchronous operations, order validation, pricing, stock reservations, invoice generation, and concurrent order processing.

## How to Run

From the `06-OrderProcessing` folder, run:

```powershell
dotnet run --project .\06-OrderProcessing\06-OrderProcessing.csproj
```

The project targets .NET 10. The `Data` folder is created under the application's output directory, for example:

```text
06-OrderProcessing\bin\Debug\net10.0\Data
```

## Project Structure

- `Models/`: Classes representing customers, products, orders, payments, coupons, and reports.
- `Enums/`: Enum types used for order and payment statuses.
- `Repository/`: Classes that read, validate, and save data. There are synchronous and asynchronous versions for comparison.
- `Service/`: Business operations such as pricing, stock reservations, order processing, and invoice generation.
- `DataFiles.cs`: Builds paths for the JSON files and output folders.
- `DataSeeder.cs`: Resets the data files and fills them with sample records.
- `JsonFileStore.cs`: Handles JSON file reads, writes, and updates.
- `JsonDefaults.cs`: Shared JSON serialization settings.
- `Log.cs`: Writes timestamped messages with the current thread ID.

## JSON Storage

Each collection is stored in its own file under `Data`:

- `customers.json`
- `products.json`
- `coupons.json`
- `incoming_orders.json`
- `orders.json`
- `payments.json`
- `emails.json`
- `email_retry.json`

Invoices are written under `Data/invoices`, and generated reports are written under `Data/reports`.

`JsonFileStore` is responsible for reading and writing the JSON files. It uses a `SemaphoreSlim` for each file. `UpdateAsync` keeps the read, update, and write steps together under the same lock so concurrent updates do not overwrite one another.

When writing a file, the data is first serialized to a temporary file. The temporary file then replaces the original file. This helps avoid leaving a partially written JSON file.

The project uses `System.Text.Json` with shared settings in `JsonDefaults`. Property names are written in camelCase, and enums are stored as text. Invalid JSON causes a `DataFileCorruptedException`.

The repositories and services simulate network or database delays. These delays are not placed inside `JsonFileStore`.

---

## Phase 1 Questions

### 1. Why are the sync and async times almost the same?

Both versions process orders one at a time. The async version uses `await`, but that does not make the orders run in parallel. It allows the thread to do other work while an operation is waiting. Since the total waiting time for each order is similar, the overall times can also be close.

### 2. Is the thread ID before an `await` always the same as after it? Why?

No. The code after an `await` may continue on a different thread, depending on the execution context and scheduler. In my logs, the thread IDs before and after an awaited operation show whether the thread changed in that run. Seeing the same ID once does not mean it will always be the same.

### 3. What does the thread do while waiting in `Task.Delay()` and `Thread.Sleep()`?

While `Task.Delay()` is waiting, the thread is available to do other work. When the delay finishes, the code continues after `await`.

With `Thread.Sleep()`, the current thread is blocked until the sleep time ends, so it cannot do other work during that time.

### 4. Why should a repository method return `Task<T>` instead of `T`?

A repository may need to wait for a file or database operation. Returning `Task<T>` lets the caller use `await` without blocking its thread while waiting. Returning `T` means the result is returned synchronously, so the caller has to wait for the operation to finish.

### 5. What is the difference between `File.ReadAllText` and `File.ReadAllTextAsync`?

`File.ReadAllText` reads the file synchronously, so the calling thread waits until the read finishes.

`File.ReadAllTextAsync` returns a `Task<string>` that can be awaited. During an asynchronous file read, a thread does not need to stay occupied while waiting for the disk operation to complete.

---

## Phase 2 - Concurrent Processing

### Pricing Comparison

I ran the sequential and concurrent pricing methods with Order #1. Both methods produced the same totals. The concurrent method took less elapsed time in this run.

| Method     |   Time | Subtotal | Discount | Shipping |  Total |
| ---------- | -----: | -------: | -------: | -------: | -----: |
| Sequential | 5.64 s |  1225.00 |   245.00 |     0.00 | 980.00 |
| Concurrent | 3.64 s |  1225.00 |   245.00 |     0.00 | 980.00 |

These measurements are from one local run, so the times can change between runs. The logs showed the discount and shipping calls starting before either result was awaited in the concurrent method.

### Stock Reservation

The stock race demo uses Orders #10 and #11, which both try to reserve the last Mechanical Keyboard.

The unsafe version reads the stock before updating it. Both orders can read a stock value of `1` and both can report a successful reservation.

The safe version performs the stock check and update inside `JsonFileStore.UpdateAsync`. The expected result is that one reservation succeeds, the other fails, and the stock ends at `0`.

### Order Processing and Invoices

`OrderProcessorV2` processes validated orders by reserving their items in order, pricing the order, generating its invoice, and saving it as completed. If a stock reservation fails, it releases items already reserved, saves the order as rejected, and returns `false`.

`InvoiceGenerator` uses `Task.Run` for the simulated CPU-heavy rendering loop, then writes the invoice file asynchronously under `Data/invoices`.

The full Phase 2 processing run and Report Generation task (8.6) still need to be run and verified before recording their final timings and report results here.

---

## Phase 2 Questions

### 1. What is the difference between starting a task and awaiting it?

Starting an operation that returns a `Task` gives me a task representing that work. The work may begin before I await it. I can start other independent operations while that task is running.

`await` asynchronously waits for that task to finish. It then gives me the result, or throws the exception stored by the task.

### 2. Does every `Task` create a new thread? Does `Task.Delay()` use a thread while waiting?

No. A `Task` represents an operation and its eventual completion; it does not always create a new thread. Asynchronous file or network operations can wait without occupying a thread.

`Task.Delay()` uses a timer while waiting. It does not keep a thread blocked for the whole delay.

### 3. What happens if you call `.Result` or `.Wait()` on an unfinished task instead of `await`? Why is it dangerous?

`.Result` and `.Wait()` block the current thread until the task finishes. This can waste a thread and, in applications with a synchronization context, can cause a deadlock if the task's continuation needs that blocked context.

This separate method demonstrates blocking. `.Wait()` and `.Result` should not be used elsewhere in the async flow:

```csharp
private static void BlockingWaitDemo()
{
    Task.Delay(500).Wait();
}
```

### 4. What happens to an exception thrown inside a task that is never awaited?

The task stores the exception. If nobody awaits the task or checks its exception, the failure may go unnoticed. The code that started the task does not automatically receive the exception at the point where the task was started.

### 5. When should you use `Task.Run()`, and when should you avoid it?

I use `Task.Run()` for CPU-bound synchronous work that would otherwise occupy the calling thread, such as the invoice rendering simulation.

I avoid wrapping naturally asynchronous I/O, such as file or database calls, in `Task.Run()`. Those APIs already provide asynchronous waiting, and wrapping them would use a thread unnecessarily.

### 6. What is the difference between `Task` and `Task<T>`?

`Task` represents an operation that finishes without returning a value.

`Task<T>` represents an operation that finishes with a value of type `T`. For example, `Task<Product?>` represents an asynchronous operation that returns a product or `null`.

### 7. Why can a `lock` block not contain an `await`?

A `lock` block must run synchronously, and C# does not allow `await` inside it. A `lock` uses a thread-bound monitor, while an async method may pause and continue later. For asynchronous mutual exclusion, `SemaphoreSlim` with `WaitAsync` can be used.

### 8. Why did the unprotected stock reservation oversell, and how did you prevent it?

Both orders read the same stock value before either one updated it. Each order saw one keyboard available and decided it could reserve it.

I prevented this by putting the stock check and decrement together inside `JsonFileStore.UpdateAsync`. Its per-file `SemaphoreSlim` makes each update wait for the previous update to finish. The next order then reads the updated stock and cannot reserve an unavailable item.

## 9.6 - Sync vs Async Comparison

**1. Does async make one order's workflow faster?**

Not necessarily. Async does not make the database or API respond faster. One order still waits for the same operations. In my run, processing 10 orders took 11.92 seconds with the sync version and 7.10 seconds with the async version, because the async version handled the waits more efficiently.

**2. Why does async scale better for many orders?**

When a sync operation waits for a database, file, or API, its thread stays blocked. Async operations can release the thread while waiting, so the application can handle more work without needing a separate blocked thread for every operation.

**3. What happens to the sync version with 1,000 orders?**

It would need to create and manage 1,000 dedicated threads if it starts one thread per order. This uses more memory and increases thread scheduling and context switching. The application may become slower or run out of available resources.

**4. Why is async essential for applications that call databases, files, and APIs?**

These operations spend much of their time waiting for external resources. Async lets the application use its threads for other work during that time, which helps it remain responsive and handle more concurrent requests.

### Results from my run

| Version | Orders | Completed |   Rejected | Unique threads observed | Elapsed |
| ------- | -----: | --------: | ---------: | ----------------------: | ------: |
| Sync    |     10 |         8 | 2 (#4, #9) |                      10 | 11.92 s |
| Async   |     10 |         8 | 2 (#4, #9) |                       5 |  7.10 s |

The `ThreadPool.ThreadCount` readings were 9 → 6 during the sync run and 6 → 9 during the async run. These readings can change as the thread pool adds or retires idle threads.
