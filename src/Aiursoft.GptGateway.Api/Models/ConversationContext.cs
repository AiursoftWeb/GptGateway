namespace Aiursoft.GptGateway.Api.Models;

public class ConversationContext
{
    public List<string> ToolsUsed { get; set; } = new();
    
    public List<string> UserMessages { get; set; } = new();
}