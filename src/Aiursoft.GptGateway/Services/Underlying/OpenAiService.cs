using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;
using Aiursoft.GptGateway.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Services.Underlying;

public class OpenAiService(
    ILogger<OpenAiService> logger,
    IOptions<UnderlyingsOptions> options,
    ChatClient client) : IUnderlyingService
{
    public string Name => "OpenAI";

    public Task<HttpResponseMessage> AskStream(OpenAiRequestModel model, CancellationToken cancellationToken)
    {
        model.Stream = true;
        var endPoint = options.Value.OpenAI.InstanceUrl.TrimEnd('/') + "/v1/chat/completions";
        logger.LogInformation("Ask OpenAI model with endpoint: {endPoint}, token is {token}", endPoint,
            options.Value.OpenAI.Token.SafeSubstring(10));
        return client.AskStream(
            model: model,
            completionApiUrl: endPoint,
            token: options.Value.OpenAI.Token,
            cancellationToken: cancellationToken);
    }

    public Task<CompletionData> AskModel(OpenAiRequestModel model, CancellationToken cancellationToken)
    {
        model.Stream = false;
        var endPoint = options.Value.OpenAI.InstanceUrl.TrimEnd('/') + "/v1/chat/completions";
        logger.LogInformation("Ask OpenAI model with endpoint: {endPoint}, token is {token}", endPoint,
            options.Value.OpenAI.Token.SafeSubstring(10));
        return client.AskModelWithRetry(
            model: model,
            completionApiUrl: endPoint,
            token: options.Value.OpenAI.Token,
            logger: logger,
            cancellationToken: cancellationToken);
    }

    public bool SupportOllamaTooling => false;
}
