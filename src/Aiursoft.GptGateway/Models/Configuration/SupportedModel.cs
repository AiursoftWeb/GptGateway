namespace Aiursoft.GptGateway.Models.Configuration;

public class SupportedModel
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string UnderlyingProvider { get; init; }
    public required string UnderlyingModel { get; init; }
    public string[] Plugins { get; init; } = [];
}
