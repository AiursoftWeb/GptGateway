using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Models.Configuration;
using Aiursoft.GptGateway.Services;
using Aiursoft.GptGateway.Services.Abstractions;
using Aiursoft.GptGateway.Services.Underlying;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Controllers;

[Route("/api")]
[Route("/v1")]
[Obsolete]
public class ProxyController(
    StreamTransformService streamTransformService,
    ILogger<ProxyController> logger,
    IEnumerable<IPlugin> plugins,
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
                Digest = GenerateHash(name),
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

    private static string GenerateHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    [HttpPost("chat")]
    [HttpPost("chat/completions")]               // 相对路径 -> /api/chat/completions
    public async Task<IActionResult> Chat([FromBody] OpenAiModel rawInput)
    {
        var usingModel = string.IsNullOrWhiteSpace(rawInput.Model)
            ? modelOptions.Value.DefaultIncomingModel
            : rawInput.Model;

        var modelConfig = modelOptions
            .Value
            .SupportedModels
            .FirstOrDefault(m => usingModel.StartsWith(m.Name, StringComparison.OrdinalIgnoreCase));

        if (modelConfig is null)
        {
            logger.LogWarning("Model not found for request with {InputModel}", rawInput.Model);
            return BadRequest("Model not found.");
        }
        else
        {
            logger.LogInformation("Using model: {Model} for request with {InputModel}", modelConfig.Name, rawInput.Model);
        }

        var underlyingService = underlyingServices
            .FirstOrDefault(s => s.Name == modelConfig.UnderlyingProvider);
        if (underlyingService is null)
        {
            logger.LogWarning("Underlying service not found for request with {InputModel}", rawInput.Model);
            return BadRequest("Underlying service not found.");
        }
        else
        {
            logger.LogInformation("Using underlying service: {Service} for request with {InputModel}", underlyingService.Name, rawInput.Model);
        }

        var context = new ConversationContext
        {
            HttpContext = HttpContext,
            RawInput = rawInput,
            ModifiedInput = rawInput.Clone(),
            Output = null
        };
        context.ModifiedInput.Model = modelConfig.UnderlyingModel;
        foreach (var plugin in modelConfig.Plugins)
        {
            var pluginService = plugins
                .FirstOrDefault(p => p.PluginName == plugin);
            if (pluginService is null)
            {
                logger.LogWarning("Plugin not found for request with {InputModel}", rawInput.Model);
                return BadRequest($"Plugin with name {plugin} not found.");
            }
            else
            {
                logger.LogInformation("Using plugin: {Plugin} for request with {InputModel}", pluginService.PluginName, rawInput.Model);
            }

            await pluginService.ProcessMessage(context, underlyingService, cancellationToken: HttpContext.RequestAborted);
        }

        if (context.RawInput.Stream == true)
        {
            var responseStream = await underlyingService.AskStream(context.ModifiedInput, cancellationToken: HttpContext.RequestAborted);

            // Use the StreamTransformService to handle the transformation if needed
            await streamTransformService.CopyProxyHttpResponse(
                HttpContext,
                responseStream,
                underlyingService.Name,  // Pass service name to identify source format
                modelConfig.Name);       // Pass target model name for Ollama format

            return new EmptyResult();
        }
        else
        {
            var response = await underlyingService.AskModel(context.ModifiedInput, cancellationToken: HttpContext.RequestAborted);
            return Ok(response);
        }
    }
}
