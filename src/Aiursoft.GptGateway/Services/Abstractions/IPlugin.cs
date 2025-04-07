using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Underlying;

namespace Aiursoft.GptGateway.Services.Abstractions;

public interface IPlugin
{
    string PluginName { get; }

    Task ProcessMessage(ConversationContext context, IUnderlyingService service);
}
