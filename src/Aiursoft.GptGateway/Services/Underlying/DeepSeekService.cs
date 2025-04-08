using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;
using Aiursoft.GptGateway.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Services.Underlying;

public class DeepSeekService(
    ILogger<DeepSeekService> logger,
    IOptions<UnderlyingsOptions> options,
    ChatClient client) : IUnderlyingService
{
    public string Name => "DeepSeek";

    public Task<HttpResponseMessage> AskStream(OpenAiModel model, CancellationToken cancellationToken)
    {
        model.Stream = true;
        var endPoint = options.Value.DeepSeek.Instance.TrimEnd('/') + "/chat/completions";
        logger.LogInformation("Ask DeepSeek stream with endpoint: {endPoint}, token is {token}", endPoint,
            options.Value.DeepSeek.Token.SafeSubstring(10));
        return client.AskStream(
            model: model,
            completionApiUrl: endPoint,
            token: options.Value.DeepSeek.Token,
            cancellationToken: cancellationToken);
    }

    public Task<CompletionData> AskModel(OpenAiModel model, CancellationToken cancellationToken)
    {
        model.Stream = false;
        var endPoint = options.Value.DeepSeek.Instance.TrimEnd('/') + "/chat/completions";
        logger.LogInformation("Ask DeepSeek model with endpoint: {endPoint}, token is {token}", endPoint,
            options.Value.DeepSeek.Token.SafeSubstring(10));
        return client.AskModel(
            model: model,
            completionApiUrl: endPoint,
            token: options.Value.DeepSeek.Token,
            cancellationToken: cancellationToken);
    }
}
