using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PostRequest;

public class ShowPluginUsageMiddleware : IPostRequestMiddleware
{
    public Task<CompletionData> PostRequest(HttpContext context, OpenAiModel model, CompletionData data, DateTime requestTime, ConversationContext conv)
    {
        var messages = string.Join("\n", conv.UserMessages); 
            
        data.Choices.FirstOrDefault()!.Message!.Content = $"{messages}\n\n{data.Choices.FirstOrDefault()!.Message!.Content}";
        return Task.FromResult(data);
    }
}