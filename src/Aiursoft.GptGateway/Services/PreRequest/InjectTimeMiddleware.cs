using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Abstractions;

namespace Aiursoft.GptGateway.Services.PreRequest;

public class InjectTimeMiddleware(ILogger<InjectTimeMiddleware> logger) : IPreRequestMiddleware
{
    public Task PreRequest(ConversationContext conv)
    {
        if (!(conv.ModifiedInput.Messages.FirstOrDefault()?.Content?.StartsWith("此时此刻的时间是") ?? false))
        {
            conv.ModifiedInput.Messages.Insert(0, new MessagesItem
            {
                Role = "assistant",
                Content = $"此时此刻的时间是 {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}。",
                IsInjected = true,
            });
        }
        
        logger.LogInformation("Time injected. Last question: {0}", conv.ModifiedInput.Messages[^1].Content);
        return Task.CompletedTask;
    }
}