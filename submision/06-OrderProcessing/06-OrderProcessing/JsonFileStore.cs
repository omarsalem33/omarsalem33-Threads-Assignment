using System.Collections.Concurrent;
using System.Text.Json;

namespace _06_OrderProcessing;

public static class JsonFileStore
{
    public static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new();

    private static SemaphoreSlim GetFileLock(string filePath)
        => _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));
    public static async Task<T> ReadAsync<T>(string filePath , CancellationToken cancellationToken) where T : new()
    {
        var semaphore =GetFileLock(filePath);
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(filePath))
                return new T();
            
            await using var stream = 
                new FileStream(filePath, FileMode.Open,FileAccess.Read , FileShare.Read, 4096,useAsync:true);
            
            try
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, JsonDefaults.JsonSerializerOptions, cancellationToken) ?? new T();
            }
            catch (JsonException ex)
            {
                throw new DataFileCorruptedException(Path.GetFileName(filePath), 
                    $"The data file '{Path.GetFileName(filePath)}' contains invalid JSON.", ex);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
    public static async Task WriteAsync<T>(string filePath, T data, CancellationToken cancellationToken = default)
    {
        var semaphore = GetFileLock(filePath);
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempFilePath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                await using (var createStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await JsonSerializer.SerializeAsync(createStream, data, JsonDefaults.JsonSerializerOptions, cancellationToken);
                }

                File.Move(tempFilePath, filePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch
                    {
                        
                    }
                }
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
    public static T Read<T>(string filePath) where T : new()
    {
        var semaphore = GetFileLock(filePath);
        semaphore.Wait();
        try
        {
            if (!File.Exists(filePath))
            {
                return new T();
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
           
            try
            {
                return JsonSerializer.Deserialize<T>(stream, JsonDefaults.JsonSerializerOptions) ?? new T();
            }
            catch (JsonException ex)
            {
                throw new DataFileCorruptedException(Path.GetFileName(filePath), 
                    $"The data file '{Path.GetFileName(filePath)}' contains invalid JSON.", ex);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
    public static void Write<T>(string filePath, T data)
    {
        var semaphore = GetFileLock(filePath);
        semaphore.Wait();
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempFilePath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

            try
            {
                using (var createStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(createStream, data, JsonDefaults.JsonSerializerOptions);
                }

                File.Move(tempFilePath, filePath, overwrite: true);
            }
            finally
            {
                if(File.Exists(tempFilePath))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch
                    {
                        
                    }
                }
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
    public static T Update<T>(string filePath, Func<T, T> update) where T : new()
    {
        var semaphore = GetFileLock(filePath);
        semaphore.Wait();
        string tempFilePath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            T current;
            if (!File.Exists(filePath))
            {
                current = new T();
            }
            else
            {
                using var readStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    current = JsonSerializer.Deserialize<T>(readStream, JsonDefaults.JsonSerializerOptions) ?? new T();
                }
                catch (JsonException ex)
                {
                    throw new DataFileCorruptedException(
                        Path.GetFileName(filePath),
                        $"The data file '{Path.GetFileName(filePath)}' contains invalid JSON.",
                        ex);
                }
            }

            T updated = update(current);
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writeStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(writeStream, updated, JsonDefaults.JsonSerializerOptions);
            }

            File.Move(tempFilePath, filePath, overwrite: true);
            return updated;
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); }
                catch { }
            }

            semaphore.Release();
        }
    }
    public static async Task<T> UpdateAsync<T>(string filePath, Func<T, T> update, CancellationToken ct) where T : new() 
    {
        var semaphore = GetFileLock(filePath);
        await semaphore.WaitAsync(ct);

        string tempFilePath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            T current;

            if (!File.Exists(filePath))
            {
                current = new T();
            }
            else
            {
                await using var readStream =
                    new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

                try
                {
                    current = await JsonSerializer.DeserializeAsync<T>(
                        readStream,
                        JsonDefaults.JsonSerializerOptions,
                        ct) ?? new T();
                }
                catch (JsonException ex)
                {
                    throw new DataFileCorruptedException(
                        Path.GetFileName(filePath),
                        $"The data file '{Path.GetFileName(filePath)}' contains invalid JSON.",
                        ex);
                }
            }

            T updated = update(current);

            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using (var writeStream =
                         new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(
                    writeStream,
                    updated,
                    JsonDefaults.JsonSerializerOptions,
                    ct);
            }

            File.Move(tempFilePath, filePath, overwrite: true);

            return updated;
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch
                {
                }
            }

            semaphore.Release();
        } 
    }
    
}
