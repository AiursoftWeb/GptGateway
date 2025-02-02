using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Models;

namespace Aiursoft.GptGateway.Services.Abstractions;

public interface IPlugin
{
    string PluginName { get; }
    
    Task<int> GetUsagePoint(OpenAiModel input);
    
    Task<string> GetPluginAppendedMessage(ConversationContext context);
}