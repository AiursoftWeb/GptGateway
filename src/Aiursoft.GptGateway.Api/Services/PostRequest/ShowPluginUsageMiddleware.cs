using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PostRequest;

public class ShowPluginUsageMiddleware : IPostRequestMiddleware
{
    public Task PostRequest(ConversationContext conv)
    {
        var messages = string.Join("\n", conv.PluginMessages); 
            
        conv.Output!.Choices.FirstOrDefault()!.Message!.Content = $"{messages}\n\n{conv.Output!.Choices.FirstOrDefault()!.Message!.Content}";
        return Task.CompletedTask;
    }
}