namespace Aiursoft.GptGateway.Models.Configuration;

public class ClickhouseOptions
{
    public bool Enabled { get; set; } = false;
    public string ConnectionString { get; set; } = string.Empty;
}
