namespace Aiursoft.GptGateway.Models.Configuration;

public class GptModelOptions
{
    public string? ApiKey { get; set; }
    public required string DefaultIncomingModel { get; set; }
    public SupportedModel[] SupportedModels { get; set; } = [];
    public int TimeoutMinutes { get; set; } = 10;
}