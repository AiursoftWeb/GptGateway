using System.Text.Json.Serialization;

namespace Aiursoft.GptGateway.Models;

public class ChoicesItemData
{
    /// <summary>
    /// The message data for this choice.
    /// </summary>
    [JsonPropertyName("message")]
    public MessageData? Message { get; set; }

    /// <summary>
    /// The reason why this choice was selected as the final choice.
    /// </summary>
    [JsonPropertyName("finish_reason")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? FinishReason { get; set; }

    /// <summary>
    /// The index of this choice in the list of choices.
    /// </summary>
    [JsonPropertyName("index")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int? Index { get; set; }
}