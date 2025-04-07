using Aiursoft.GptClient.Abstractions;

namespace Aiursoft.GptGateway.Models;

public class ConversationContext
{
    public DateTime RequestTime { get; init; } = DateTime.UtcNow;

    public required HttpContext HttpContext { get; init; }

    public required OpenAiModel ModifiedInput { get; init; }

    public required OpenAiModel RawInput { get; init; }

    public CompletionData? Output { get; set; }
}
