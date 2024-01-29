using System.Text.Json.Serialization;

namespace Aiursoft.GptGateway.Api.Services;

public class UsageData
{
    /// <summary>
    /// The number of prompt tokens used in the request.
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; set; }

    /// <summary>
    /// The number of completion tokens generated in the response.
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; set; }

    /// <summary>
    /// The total number of tokens used in the request and generated in the response.
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; set; }

    /// <summary>
    /// The number of tokens in the prompt before any adjustments were made.
    /// </summary>
    [JsonPropertyName("pre_token_count")]
    public int? PreTokenCount { get; set; }

    /// <summary>
    /// The total number of tokens in the prompt before any adjustments were made.
    /// </summary>
    [JsonPropertyName("pre_total")]
    public int? PreTotal { get; set; }

    /// <summary>
    /// The total number of tokens used in the response after adjustments were made.
    /// </summary>
    [JsonPropertyName("adjust_total")]
    public int? AdjustTotal { get; set; }

    /// <summary>
    /// The final total number of tokens in the response.
    /// </summary>
    [JsonPropertyName("final_total")]
    public int? FinalTotal { get; set; }
}