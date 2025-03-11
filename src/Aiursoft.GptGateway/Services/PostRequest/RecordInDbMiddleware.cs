﻿using System.Text.Encodings.Web;
using System.Text.Json;
using Aiursoft.Canon;
using Aiursoft.GptGateway.Entities;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Models.Database;
using Aiursoft.GptGateway.Services.Abstractions;

namespace Aiursoft.GptGateway.Services.PostRequest;

public class RecordInDbMiddleware(
    ILogger<RecordInDbMiddleware> logger,
    CanonQueue canonQueue)
    : IPostRequestMiddleware
{
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
            Answer = conv.Output!.GetFullContent(),
            ToolsUsed = string.Join(", ", conv.ToolsUsed.Select(t => t.PluginName)),
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
        
        canonQueue.QueueWithDependency<GptGatewayDbContext>(async db =>
        {
            await db.UserConversations.AddAsync(userConversation);
            await db.SaveChangesAsync();
            
            logger.LogInformation("Recorded a conversation from {Ip}.", userConversation.RequestIpAddress);
        });
        return Task.CompletedTask;
    }
}