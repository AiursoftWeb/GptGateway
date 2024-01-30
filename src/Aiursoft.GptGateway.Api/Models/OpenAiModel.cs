using System.Text.Json.Serialization;

namespace Aiursoft.GptGateway.Api.Models;

public class OpenAiModel
{
    [JsonPropertyName("messages")] public List<MessagesItem> Messages { get; set; } = new();

    [JsonPropertyName("stream")] public bool? Stream { get; set; } = false;

    [JsonPropertyName("model")] public string? Model { get; set; } = "gpt-4-0613";

    [JsonPropertyName("temperature")] public double? Temperature { get; set; } = 0.5;

    [JsonPropertyName("presence_penalty")] public int? PresencePenalty { get; set; } = 0;
}