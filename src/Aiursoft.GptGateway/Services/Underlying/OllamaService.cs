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

    public Task<HttpResponseMessage> AskStream(OpenAiModel model)
    {
        model.Stream = true;
        var endPoint = options.Value.Ollama.Instance.TrimEnd('/') + "/api/chat";
        logger.LogInformation("Ask Ollama model with endpoint: {endPoint}", endPoint);
        return client.AskStream(
            model: model,
            completionApiUrl: endPoint,
            token: string.Empty);
    }

    public Task<CompletionData> AskModel(OpenAiModel model)
    {
        model.Stream = false;
        var endPoint = options.Value.Ollama.Instance.TrimEnd('/') + "/api/chat";
        logger.LogInformation("Ask Ollama model with endpoint: {endPoint}", endPoint);
        return client.AskModel(
            model: model,
            completionApiUrl: endPoint,
            token: string.Empty);
    }
}