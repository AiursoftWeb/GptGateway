using System.Text.Json.Serialization;

namespace Aiursoft.GptGateway.Api.Services;

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