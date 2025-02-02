using System.Text.Json.Serialization;
using Aiursoft.GptClient.Abstractions;

namespace Aiursoft.GptGateway.Models;

/// <summary>
/// Represents the response data from the OpenAI API for a text completion request.
/// </summary>
public class TextCompletionResponseData
{
    /// <summary>
    /// The completion data for this response.
    /// </summary>
    [JsonPropertyName("completion")]
    public CompletionData? Completion { get; set; }
}