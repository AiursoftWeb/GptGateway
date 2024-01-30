using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services.Abstractions;

public interface IPreRequestMiddleware
{
    Task<OpenAiModel> PreRequest(HttpContext context, OpenAiModel model);
}