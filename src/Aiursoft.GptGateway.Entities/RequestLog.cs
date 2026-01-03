namespace Aiursoft.GptGateway.Entities;

public class RequestLog
{
    public string IP { get; set; } = string.Empty;
    public int ConversationMessageCount { get; set; }
    public string LastQuestion { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public bool Success { get; set; }
    public double Duration { get; set; }
    public string Thinking { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime RequestTime { get; set; }
}
