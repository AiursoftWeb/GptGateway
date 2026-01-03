using Aiursoft.GptGateway.Data;
using Aiursoft.GptGateway.Models.Configuration;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Extensions;

public static class ClickhouseExtensions
{
    public static IServiceCollection AddClickhouse(this IServiceCollection services)
    {
        services.AddScoped<ClickhouseDbContext>();
        return services;
    }

    public static async Task InitClickhouseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<ClickhouseOptions>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ClickhouseDbContext>>();

        if (!options.CurrentValue.Enabled)
        {
            return;
        }

        try
        {
            await using var connection = new ClickHouseConnection(options.CurrentValue.ConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS RequestLogs (
                    IP String,
                    ConversationMessageCount Int32,
                    LastQuestion String,
                    Model String,
                    Success UInt8,
                    Duration Float64,
                    Thinking String,
                    Answer String,
                    RequestTime DateTime
                ) ENGINE = MergeTree()
                ORDER BY RequestTime";
            await command.ExecuteNonQueryAsync();
            logger.LogInformation("Clickhouse table initialized.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to initialize Clickhouse table.");
        }
    }
}
