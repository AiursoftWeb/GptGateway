using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Models.Configuration;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;
using Microsoft.Extensions.Options;
using System.Data;

namespace Aiursoft.GptGateway.Services;

public class ClickhouseService(
    IOptionsMonitor<ClickhouseOptions> options,
    ILogger<ClickhouseService> logger)
{
    public async Task Init()
    {
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

    public async Task Log(RequestLog log)
    {
        if (!options.CurrentValue.Enabled)
        {
            return;
        }

        try
        {
            await using var connection = new ClickHouseConnection(options.CurrentValue.ConnectionString);
            await connection.OpenAsync();

            using var bulkCopy = new ClickHouseBulkCopy(connection)
            {
                DestinationTableName = "RequestLogs",
                BatchSize = 1
            };

            var data = new List<object[]>
            {
                new object[]
                {
                    log.IP,
                    log.ConversationMessageCount,
                    log.LastQuestion,
                    log.Model,
                    log.Success ? (byte)1 : (byte)0,
                    log.Duration,
                    log.Thinking,
                    log.Answer,
                    log.RequestTime
                }
            };

            await bulkCopy.WriteToServerAsync(data);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to log to Clickhouse.");
        }
    }
}
