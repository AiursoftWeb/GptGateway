using Aiursoft.GptClient.Abstractions;

namespace Aiursoft.GptGateway.Models;

public class ConversationContext
{
    public DateTime RequestTime { get; init; } = DateTime.UtcNow;

    public required HttpContext HttpContext { get; init; }

    public required OpenAiRequestModel ModifiedInput { get; init; }

    public required OpenAiRequestModel RawInput { get; init; }

    public CompletionData? Output { get; set; }
}
