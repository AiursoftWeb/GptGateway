using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PreRequest;

public class InjectTimeMiddleware : IPreRequestMiddleware
{
    public Task<OpenAiModel> PreRequest(HttpContext context, OpenAiModel model, ConversationContext conv)
    {
        if (!(model.Messages.FirstOrDefault()?.Content?.StartsWith("此时此刻的时间是") ?? false))
        {
            model.Messages.Insert(0, new MessagesItem
            {
                Role = "user",
                Content = $"此时此刻的时间是 {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}。"
            });
        }
        return Task.FromResult(model);
    }
}