using System.Text.Encodings.Web;
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
            Questions = JsonSerializer.Serialize(model.Messages.Select(m => m.Content), new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }),
            Answer = data.Choices.FirstOrDefault()?.Message?.Content ?? "No answer.",
            Duration = DateTime.UtcNow - requestTime,
            ConversationTime = DateTime.UtcNow,
            PromptTokens = data.Usage?.PromptTokens ?? 0,
            CompletionTokens = data.Usage?.CompletionTokens ?? 0,
            TotalTokens = data.Usage?.TotalTokens ?? 0,
            PreTokenCount = data.Usage?.PreTokenCount ?? 0,
            PreTotal = data.Usage?.PreTotal ?? 0,
            AdjustTotal = data.Usage?.AdjustTotal ?? 0,
            FinalTotal = data.Usage?.FinalTotal ?? 0,
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