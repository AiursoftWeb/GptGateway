using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Abstractions;

namespace Aiursoft.GptGateway.Services.PostRequest;

public class MockModelMiddleware : IPostRequestMiddleware
{
    public Task PostRequest(ConversationContext conv)
    {
        conv.Output!.Model = "aiursoft-chat";
        return Task.CompletedTask;
    }
}