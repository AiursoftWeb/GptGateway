using System.Text.Json;
using Aiursoft.Canon;
using Aiursoft.GptGateway.Api.Data;
using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Models.Database;

namespace Aiursoft.GptGateway.Api.Services;

public class RecordInDbMiddleware : IPostRequestMiddleware
{
    private readonly ILogger<RecordInDbMiddleware> _logger;
    private readonly CanonQueue _canonQueue;
    
    public RecordInDbMiddleware(
        ILogger<RecordInDbMiddleware> logger,
        CanonQueue canonQueue)
    {
        _logger = logger;
        _canonQueue = canonQueue;
    }
    
    public Task<CompletionData> PostRequest(HttpContext context, OpenAiModel model, CompletionData data, DateTime requestTime)
    {
        var userConversation = new UserConversation
        {
            RequestIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            RequestUserAgent = context.Request.Headers["User-Agent"].ToString(),
            Questions = JsonSerializer.Serialize(model.Messages.Select(m => m.Content)),
            Answer = data.Choices.FirstOrDefault()?.Message?.Content ?? "No answer.",
            Duration = DateTime.UtcNow - requestTime,
            ConversationTime = DateTime.UtcNow
        };
        
        _canonQueue.QueueWithDependency<GptGatewayDbContext>(async db =>
        {
            await db.UserConversations.AddAsync(userConversation);
            await db.SaveChangesAsync();
            
            _logger.LogInformation("Recorded a conversation from {Ip}.", userConversation.RequestIpAddress);
        });
        
        return Task.FromResult(data);
    }
}