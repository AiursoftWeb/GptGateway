using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;
using Aiursoft.GptGateway.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Services.Underlying;

public class OllamaService(
    ILogger<OllamaService> logger,
    IOptions<UnderlyingsOptions> options,
    ChatClient client) : IUnderlyingService
{
    public string Name => "Ollama";

    public Task<HttpResponseMessage> AskStream(OpenAiRequestModel model, CancellationToken cancellationToken)
    {
        model.Stream = true;
        var endPoint = options.Value.Ollama.Instance.TrimEnd('/') + "/api/chat";
        logger.LogInformation("Ask Ollama model streamly with endpoint: {endPoint}.", endPoint);
        return client.AskStream(
            model: model,
            completionApiUrl: endPoint,
            token: string.Empty,
            cancellationToken: cancellationToken);
    }

    public Task<CompletionData> AskModel(OpenAiRequestModel model, CancellationToken cancellationToken)
    {
        model.Stream = false;
        var endPoint = options.Value.Ollama.Instance.TrimEnd('/') + "/api/chat";
        logger.LogInformation("Ask Ollama model with endpoint: {endPoint}.", endPoint);
        return client.AskModelWithRetry(
            model: model,
            completionApiUrl: endPoint,
            token: string.Empty,
            logger: logger,
            cancellationToken: cancellationToken);
    }

    public bool SupportOllamaTooling => true;
}
