namespace Aiursoft.GptGateway.Models.Configuration;

public class DeepSeekOptions
{
    public bool IsEnabled { get; init; }
    public required string Token { get; init; }
    public required string Instance { get; init; }
}