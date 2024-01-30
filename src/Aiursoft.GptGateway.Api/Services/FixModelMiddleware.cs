using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services;

public class FixModelMiddleware : IPreRequestMiddleware
{
    public Task<OpenAiModel> PreRequest(HttpContext context, OpenAiModel model)
    {
        model.Messages = model.Messages.TakeLast(6).ToList();
        model.Stream = false;
        model.Model = "gpt-3.5-turbo-16k";
        return Task.FromResult(model);
    }
}