using Newtonsoft.Json;

namespace Aiursoft.GptGateway.Models;

public class ModelDetails
{
    [JsonProperty("parent_model")]
    public required string ParentModel { get; init; }

    [JsonProperty("format")]
    public required string Format { get; init; }

    [JsonProperty("family")]
    public required string Family { get; init; }

    [JsonProperty("families")]
    public required string[] Families { get; init; }

    [JsonProperty("parameter_size")]
    public required string ParameterSize { get; init; }

    [JsonProperty("quantization_level")]
    public required string QuantizationLevel { get; init; }
}
