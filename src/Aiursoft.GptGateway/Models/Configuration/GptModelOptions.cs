namespace Aiursoft.GptGateway.Models.Configuration;

public class GptModelOptions
{
    public string? ApiKey { get; set; }
    public required string DefaultIncomingModel { get; set; }
    public required List<SupportedModel> SupportedModels { get; set; }
}