using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Abstractions;

namespace Aiursoft.GptGateway.Services.PreRequest;

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