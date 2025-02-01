using Aiursoft.GptGateway.Models;

namespace Aiursoft.GptGateway.Services.Abstractions;

public interface IPostRequestMiddleware
{
    Task PostRequest(ConversationContext conv);
}