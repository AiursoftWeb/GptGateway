namespace Aiursoft.GptGateway.Models.Configuration;

public class UnderlyingsOptions
{
    public required OpenAIOptions OpenAI { get; init; }
    public required OllamaOptions Ollama { get; init; }
    public required DeepSeekOptions DeepSeek { get; init; }
}