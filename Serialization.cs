using System.Text.Json;

namespace api.sdk;

public static class Serialization
{
    public static JsonSerializerOptions Settings => new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}