using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services.Abstractions;

public interface IPlugin
{
    string PluginName { get; }
    
    Task<int> GetUsagePoint(OpenAiModel model);
    
    Task<string> GetPluginAppendedMessage(string question, ConversationContext context);
}