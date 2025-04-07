using Newtonsoft.Json;

namespace Aiursoft.GptGateway.Models;

public class ModelTag
{
    [JsonProperty("name")]
    public required string Name { get; init; }

    [JsonProperty("model")]
    public required string Model { get; init; }

    [JsonProperty("modified_at")]
    public required DateTime ModifiedAt { get; init; }

    [JsonProperty("size")]
    public required long Size { get; init; }

    [JsonProperty("digest")]
    public required string Digest { get; init; }

    [JsonProperty("details")]
    public required ModelDetails Details { get; init; }
}
