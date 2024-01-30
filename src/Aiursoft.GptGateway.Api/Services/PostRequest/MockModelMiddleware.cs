using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PostRequest;

public class MockModelMiddleware : IPostRequestMiddleware
{
    public Task<CompletionData> PostRequest(HttpContext context, OpenAiModel model, CompletionData data, DateTime requestTime, ConversationContext conv)
    {
        data.Model = "aiursoft-chat";
        return Task.FromResult(data);
    }
}