using System.Text.Encodings.Web;
using System.Text.Json;
using Aiursoft.Canon;
using Aiursoft.GptGateway.Api.Data;
using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Models.Database;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PostRequest;

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
    
    public Task PostRequest(ConversationContext conv)
    {
        var userConversation = new UserConversation
        {
            RequestIpAddress = conv.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            RequestUserAgent = conv.HttpContext.Request.Headers["User-Agent"].ToString(),
            Questions = JsonSerializer.Serialize(conv.RawInput.Messages, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }),
            LastQuestion = conv.RawInput.Messages.LastOrDefault()?.Content ?? "No question.",
            Answer = conv.Output!.Choices.FirstOrDefault()?.Message?.Content ?? "No answer.",
            Duration = DateTime.UtcNow - conv.RequestTime,
            ConversationTime = DateTime.UtcNow,
            PromptTokens = conv.Output!.Usage?.PromptTokens ?? 0,
            CompletionTokens = conv.Output!.Usage?.CompletionTokens ?? 0,
            TotalTokens = conv.Output!.Usage?.TotalTokens ?? 0,
            PreTokenCount = conv.Output!.Usage?.PreTokenCount ?? 0,
            PreTotal = conv.Output!.Usage?.PreTotal ?? 0,
            AdjustTotal = conv.Output!.Usage?.AdjustTotal ?? 0,
            FinalTotal = conv.Output!.Usage?.FinalTotal ?? 0,
        };
        
        _canonQueue.QueueWithDependency<GptGatewayDbContext>(async db =>
        {
            await db.UserConversations.AddAsync(userConversation);
            await db.SaveChangesAsync();
            
            _logger.LogInformation("Recorded a conversation from {Ip}.", userConversation.RequestIpAddress);
        });
        return Task.CompletedTask;
    }
}