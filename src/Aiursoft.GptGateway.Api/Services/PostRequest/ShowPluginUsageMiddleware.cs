using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PostRequest;

public class ShowPluginUsageMiddleware : IPostRequestMiddleware
{
    public Task PostRequest(ConversationContext conv)
    {
        var messages = string.Join("\n", conv.PluginMessages);

        conv.Output!.SetContent($"{messages}\n\n{conv.Output!.GetContent()}");
        return Task.CompletedTask;
    }
}