using System.Text.Json.Serialization;

namespace Aiursoft.GptGateway.Api.Models;

public class MessageData
{
    /// <summary>
    /// The role of the message, such as "user" or "assistant".
    /// </summary>
    [JsonPropertyName("role")]
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? Role { get; set; }

    /// <summary>
    /// The content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}