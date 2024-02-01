using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PostRequest;

public class MockModelMiddleware : IPostRequestMiddleware
{
    public Task PostRequest(ConversationContext conv)
    {
        conv.Output!.Model = "aiursoft-chat";
        return Task.CompletedTask;
    }
}