using System.Text.Json.Serialization;

namespace Aiursoft.GptGateway.Api.Services;

public class MessagesItem
{
    [JsonPropertyName("role")] public string? Role { get; set; }

    [JsonPropertyName("content")] public string? Content { get; set; }
}