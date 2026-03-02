using Newtonsoft.Json;

namespace Aiursoft.GptGateway.Models;

/// <summary>
/// Ollama /api/embed request (batch embedding, new API).
/// </summary>
public class OllamaEmbedRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("input")]
    public object? Input { get; set; }

    [JsonProperty("options")]
    public OllamaRequestOptions? Options { get; set; }

    [JsonProperty("keep_alive")]
    public string? KeepAlive { get; set; }
}

/// <summary>
/// Ollama /api/embed response.
/// </summary>
public class OllamaEmbedResponse
{
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("embeddings")]
    public List<List<double>> Embeddings { get; set; } = [];

    [JsonProperty("total_duration")]
    public long TotalDuration { get; set; }

    [JsonProperty("load_duration")]
    public long LoadDuration { get; set; }

    [JsonProperty("prompt_eval_count")]
    public int PromptEvalCount { get; set; }
}

/// <summary>
/// Ollama /api/embeddings request (legacy single embedding API).
/// </summary>
public class OllamaEmbeddingsRequest
{
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    [JsonProperty("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonProperty("options")]
    public OllamaRequestOptions? Options { get; set; }

    [JsonProperty("keep_alive")]
    public string? KeepAlive { get; set; }
}

/// <summary>
/// Ollama /api/embeddings response (legacy).
/// </summary>
public class OllamaEmbeddingsResponse
{
    [JsonProperty("embedding")]
    public List<double> Embedding { get; set; } = [];
}
