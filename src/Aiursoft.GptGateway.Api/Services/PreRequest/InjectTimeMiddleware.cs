using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PreRequest;

public class InjectTimeMiddleware : IPreRequestMiddleware
{
    private readonly ILogger<InjectTimeMiddleware> _logger;

    public InjectTimeMiddleware(ILogger<InjectTimeMiddleware> logger)
    {
        _logger = logger;
    }
    
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
        
        _logger.LogInformation("Time injected. Last question: {0}", conv.ModifiedInput.Messages[^1].Content);
        return Task.CompletedTask;
    }
}