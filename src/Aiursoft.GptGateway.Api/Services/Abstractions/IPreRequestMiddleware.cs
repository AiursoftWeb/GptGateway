using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services.Abstractions;

public interface IPreRequestMiddleware
{
    Task PreRequest(ConversationContext conv);
}