using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PreRequest;

// TODO: When we can correctly split the token, remove this.
public class TrimInputMiddleware : IPreRequestMiddleware
{
    public Task PreRequest(ConversationContext conv)
    {
        conv.ModifiedInput.Messages = conv.ModifiedInput.Messages.TakeLast(6).ToList();
        conv.ModifiedInput.Stream = false;
        return Task.CompletedTask;
    }
}