namespace Aiursoft.GptGateway.Models.Configuration;

public class OllamaOptions
{
    public bool IsEnabled { get; init; }
    public required string Instance { get; init; }
}