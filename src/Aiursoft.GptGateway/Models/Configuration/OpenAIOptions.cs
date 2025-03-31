namespace Aiursoft.GptGateway.Models.Configuration;

public class OpenAIOptions
{
    public bool IsEnabled { get; init; }
    public required string Token { get; init; }
    public required string InstanceUrl { get; init; }
}
