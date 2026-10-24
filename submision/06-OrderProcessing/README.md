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