using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Models.Configuration;
using Aiursoft.GptGateway.Services.Underlying;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Aiursoft.GptGateway.Controllers;

[Route("/api")]
[Obsolete]
public class ProxyController(
    IEnumerable<IUnderlyingService> underlyingServices,
    IOptions<GptModelOptions> modelOptions) : ControllerBase
{
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        return Ok(new
        {
            version = "0.6.3"
        });
    }

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
                Size = 4683075271,
                Digest = "0a8c266910232fd3291e71e5ba1e058cc5af9d411192cf88b6d30e92b6e73163",
                Details = new ModelDetails
                {
                    ParentModel = string.Empty,
                    Format = "gguf",
                    Family = "qwen2",
                    Families = [
                        "qwen2"
                    ],
                    ParameterSize = "32.8B",
                    QuantizationLevel = "Q4_K_M"
                }
            })
            .ToArray();
        return Ok(new
        {
            models = modelTags
        });
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] OpenAiModel rawInput)
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

        var underlyingService = underlyingServices
            .FirstOrDefault(s => s.Name == modelConfig.UnderlyingProvider);
        if (underlyingService is null)
        {
            return BadRequest("Underlying service not found.");
        }

        var context = new ConversationContext
        {
            HttpContext = HttpContext,
            RawInput = rawInput,
            ModifiedInput = rawInput.Clone(),
            Output = null
        };
        context.ModifiedInput.Model = modelConfig.UnderlyingModel;

        if (context.RawInput.Stream == true)
        {
            var responseStream = await underlyingService.AskStream(context.ModifiedInput);
            await CopyProxyHttpResponse(HttpContext, responseStream);
            return new EmptyResult();
        }
        else
        {
            var response = await underlyingService.AskModel(context.ModifiedInput);
            return Ok(response);
        }
    }

    private static async Task CopyProxyHttpResponse(HttpContext context, HttpResponseMessage responseMessage)
    {
        if (responseMessage == null)
        {
            throw new ArgumentNullException(nameof(responseMessage));
        }

        var response = context.Response;

        response.StatusCode = (int)responseMessage.StatusCode;
        foreach (var header in responseMessage.Headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }

        // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
        response.Headers.Remove("transfer-encoding");

        using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
        {
            var streamCopyBufferSize = 81920;
            await responseStream.CopyToAsync(response.Body, streamCopyBufferSize, context.RequestAborted);
        }
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
