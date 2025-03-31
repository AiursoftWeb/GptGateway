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

    public Task<HttpResponseMessage> AskStream(OpenAiModel model)
    {
        model.Stream = true;
        var endPoint = options.Value.OpenAI.InstanceUrl.TrimEnd('/') + "/v1/chat/completions";
        logger.LogInformation("Ask OpenAI model with endpoint: {endPoint}, token is {token}", endPoint,
            options.Value.OpenAI.Token.SafeSubstring(10));
        return client.AskStream(
            model: model,
            completionApiUrl: endPoint,
            token: options.Value.OpenAI.Token);
    }

    public Task<CompletionData> AskModel(OpenAiModel model)
    {
        model.Stream = false;
        var endPoint = options.Value.OpenAI.InstanceUrl.TrimEnd('/') + "/v1/chat/completions";
        logger.LogInformation("Ask OpenAI model with endpoint: {endPoint}, token is {token}", endPoint,
            options.Value.OpenAI.Token.SafeSubstring(10));
        return client.AskModel(
            model: model,
            completionApiUrl: endPoint,
            token: options.Value.OpenAI.Token);
    }
}
