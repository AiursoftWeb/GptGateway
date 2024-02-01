using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services.Abstractions;

public interface IPostRequestMiddleware
{
    Task PostRequest(ConversationContext conv);
}