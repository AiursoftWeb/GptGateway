using Aiursoft.GptClient.Abstractions;
using Newtonsoft.Json;

namespace Aiursoft.GptGateway.Models;

public class OllamaRequestOptions
{
    [JsonProperty("num_ctx")]
    public int? NumCtx { get; set; }

    [JsonProperty("temperature")]
    public float? Temperature { get; set; }

    [JsonProperty("top_p")]
    public float? TopP { get; set; }

    [JsonProperty("top_k")]
    public int? TopK { get; set; }
}

public class OllamaRequestModelOverride : OllamaRequestModel
{
    [JsonProperty("think")]
    public bool? Think { get; set; }

    [JsonProperty("options")]
    public OllamaRequestOptions? Options { get; set; }
}
