using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Services.Abstractions;

namespace Aiursoft.GptGateway.Models;

public class ConversationContext
{
    public DateTime RequestTime { get; init; } = DateTime.UtcNow;
    
    public List<IPlugin> ToolsUsed { get; init; } = new();
    
    public List<string> PluginMessages { get; init; } = new();
    
    public required HttpContext HttpContext { get; init; }
    
    public required OpenAiModel ModifiedInput { get; init; }
    
    public required OpenAiModel RawInput { get; init; }
    
    public CompletionData? Output { get; set; }
}
