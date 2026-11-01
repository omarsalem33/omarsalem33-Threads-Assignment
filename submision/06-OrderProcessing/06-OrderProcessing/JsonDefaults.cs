using System.Text.Json;
using System.Text.Json.Serialization;

namespace _06_OrderProcessing;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}