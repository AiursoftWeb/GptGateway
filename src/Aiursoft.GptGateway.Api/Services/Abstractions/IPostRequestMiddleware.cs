using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services.Abstractions;

public interface IPostRequestMiddleware
{
    Task<CompletionData> PostRequest(HttpContext context, OpenAiModel model, CompletionData data, DateTime requestTime);
}