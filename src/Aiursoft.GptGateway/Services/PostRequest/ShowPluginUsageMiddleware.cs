using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Abstractions;

namespace Aiursoft.GptGateway.Services.PostRequest;

public class ShowPluginUsageMiddleware : IPostRequestMiddleware
{
    public Task PostRequest(ConversationContext conv)
    {
        var messages = string.Join("\n", conv.PluginMessages);

        // The think part will be removed.
        conv.Output!.SetContent($"{messages}\n\n{conv.Output!.GetAnswerPart()}");
        return Task.CompletedTask;
    }
}