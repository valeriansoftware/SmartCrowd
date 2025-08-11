using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartCrowd.Core.Utils.Serialization;

public static class JsonSerialization
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    public static string ToJson<T>(T value, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(value, options ?? DefaultOptions);
    }

    public static T? FromJson<T>(string json, JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
    }
}

