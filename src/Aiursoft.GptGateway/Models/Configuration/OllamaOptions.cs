namespace Aiursoft.GptGateway.Models.Configuration;

public class OllamaOptions
{
    public required string Instance { get; init; }

    public int? OverrideNumCtx { get; init; }
}
