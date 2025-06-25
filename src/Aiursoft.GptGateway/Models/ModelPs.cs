using Newtonsoft.Json;

namespace Aiursoft.GptGateway.Models;

public class ModelPs
{
    [JsonProperty("name")]
    public required string Name { get; init; }

    [JsonProperty("model")]
    public required string Model { get; init; }

    [JsonProperty("size")]
    public required long Size { get; init; }

    [JsonProperty("digest")]
    public required string Digest { get; init; }

    [JsonProperty("details")]
    public required ModelDetails Details { get; init; }

    [JsonProperty("expires_at")]
    public required DateTime ExpiresAt { get; init; }

    [JsonProperty("size_vram")]
    public required long SizeVram { get; init; }
}
