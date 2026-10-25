# 06 Mini Project - Order Processing System

## JsonFileStore

In this project, each data collection is stored in a JSON file inside the `Data` folder. I created the `JsonFileStore` class to be the only class responsible for reading and writing JSON files.

The class provides async methods for common operations:

- `ReadAsync<T>` reads the data. If the file does not exist, it returns an empty collection.
- `WriteAsync<T>` writes the data.
- `UpdateAsync<T>` reads, updates, and saves the data as one operation, so simultaneous updates to the same file do not overwrite each other.

The class also provides `Read<T>` and `Write<T>` for the synchronous versions, which I will use to compare them with the async versions.

All async methods accept a `CancellationToken`, so I can cancel an operation. I used `SemaphoreSlim` to control access to the same file during read and write operations.

When writing, the data is first saved to a temporary file. After the write is complete, the temporary file replaces the original file. This helps prevent the original file from being left with incomplete JSON if a problem occurs during writing.

I used `System.Text.Json` with shared settings in `JsonDefaults`. These settings write property names in camelCase and store enums as text. If a file contains invalid JSON, `JsonFileStore` throws a `DataFileCorruptedException` whose message includes the file name.

There are no simulated delays inside `JsonFileStore`. Delays belong in the repositories and services.

---

## Phase 1 Questions

### 1. Why are the sync and async times almost the same?

Both versions process the orders one at a time The async version uses `await` but that does not make the orders run in parallel It mainly allows the thread to do other work while waiting Since the total waiting time for each order is similar the overall times are also close

### 2. Is the thread ID before an `await` always the same as after it? Why?

No The code after an `await` may continue on a different thread depending on the execution context and scheduler In my logs the thread IDs before and after the awaited operation show whether the thread changed in that run Getting the same ID once does not guarantee it will always be the same

### 3. What does the thread do while waiting in `Task.Delay()` and `Thread.Sleep()`?

While `Task.Delay()` is waiting, the thread is available to do other work After the delay finishes, the code continues after `await`

With `Thread.Sleep()` the current thread is blocked until the sleep time ends so it cannot do other work during that time

### 4. Why should a repository method return `Task<T>` instead of `T`?

A repository may need to wait for a file or database operation Returning `Task<T>` lets the caller use `await` without blocking its thread while waiting Returning `T` means the result is returned synchronously so the caller has to wait for the operation to finish

### 5. What is the difference between `File.ReadAllText` and `File.ReadAllTextAsync`?

`File.ReadAllText` reads the file synchronously so the calling thread waits until the read finishes

`File.ReadAllTextAsync` returns a `Task<string>` that can be awaited. During an asynchronous file read, the thread does not need to stay occupied while waiting for the disk operation to complete

---
