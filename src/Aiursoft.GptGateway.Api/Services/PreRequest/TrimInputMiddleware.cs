using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PreRequest;

public class TrimInputMiddleware : IPreRequestMiddleware
{
    public Task PreRequest(ConversationContext conv)
    {
        conv.ModifiedInput.Messages = conv.ModifiedInput.Messages.TakeLast(6).ToList();
        conv.ModifiedInput.Stream = false;
        return Task.CompletedTask;
    }
}