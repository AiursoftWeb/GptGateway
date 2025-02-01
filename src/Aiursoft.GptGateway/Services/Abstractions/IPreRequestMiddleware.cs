using Aiursoft.GptGateway.Models;

namespace Aiursoft.GptGateway.Services.Abstractions;

public interface IPreRequestMiddleware
{
    Task PreRequest(ConversationContext conv);
}