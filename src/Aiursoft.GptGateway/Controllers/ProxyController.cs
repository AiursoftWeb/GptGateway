using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Models.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aiursoft.GptGateway.Controllers;

[Route("/api")]
[Obsolete]
public class ProxyController(
    IOptions<GptModelOptions> modelOptions) : ControllerBase
{
    [HttpGet("tags")]
    public IActionResult GetTags()
    {
        var modelTags = modelOptions
            .Value
            .SupportedModels
            .Select(t => t.Name)
            .Select(name => new ModelTag
            {
                Name = name,
                Model = name,
                ModifiedAt = DateTime.UtcNow,
                Size = 1,
                Digest = name.GetHashCode().ToString(),
                Details = new ModelDetails
                {
                    ParentModel = string.Empty,
                    Format = string.Empty,
                    Family = string.Empty,
                    Families = [],
                    ParameterSize = string.Empty,
                    QuantizationLevel = "Q4_K_M"
                }
            });
        return Ok(modelTags);
    }

    [HttpPost("chat")]
    public IActionResult Chat([FromBody] OpenAiModel rawInput)
    {
        var usingModel = string.IsNullOrWhiteSpace(rawInput.Model)
            ? modelOptions.Value.DefaultIncomingModel
            : rawInput.Model;

        var modelConfig = modelOptions
            .Value
            .SupportedModels
            .FirstOrDefault(m => m.Name == usingModel);

        if (modelConfig is null)
        {
            return BadRequest("Model not found.");
        }

        throw new NotImplementedException();
    }
}

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
