namespace Aiursoft.GptGateway.Models.Configuration;

public class GptModelOptions
{
    public required string DefaultIncomingModel { get; set; }
    public required List<SupportedModel> SupportedModels { get; set; }
}