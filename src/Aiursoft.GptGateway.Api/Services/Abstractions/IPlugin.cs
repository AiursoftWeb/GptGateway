namespace Aiursoft.GptGateway.Api.Services.Abstractions;

public interface IPlugin
{
    
    Task<int> GetUsagePoint(string question);
    
    Task<string> GetPluginAppendedMessage(string question);
}