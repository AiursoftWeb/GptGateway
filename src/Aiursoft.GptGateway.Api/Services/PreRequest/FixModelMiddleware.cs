using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PreRequest;

public class FixModelMiddleware : IPreRequestMiddleware
{
    public Task<OpenAiModel> PreRequest(HttpContext context, OpenAiModel model)
    {
        model.Messages = model.Messages.TakeLast(6).ToList();
        model.Stream = false;
        return Task.FromResult(model);
    }
}