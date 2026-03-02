namespace Aiursoft.GptGateway.Models.Configuration;

public class EmbeddingModel
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string UnderlyingModel { get; init; }
    public OllamaRequestOptions? Options { get; init; }
}
