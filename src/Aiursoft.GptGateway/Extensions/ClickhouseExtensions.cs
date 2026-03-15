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
            
            var tableName = "RequestLogs";
            var properties = typeof(Entities.RequestLog).GetProperties();
            var columns = new List<string>();

            foreach (var prop in properties)
            {
                var type = prop.PropertyType;
                var chType = type switch
                {
                    _ when type == typeof(string) => "String",
                    _ when type == typeof(int) => "Int32",
                    _ when type == typeof(uint) => "UInt32",
                    _ when type == typeof(long) => "Int64",
                    _ when type == typeof(ulong) => "UInt64",
                    _ when type == typeof(float) => "Float32",
                    _ when type == typeof(double) => "Float64",
                    _ when type == typeof(bool) => "UInt8",
                    _ when type == typeof(DateTime) => "DateTime",
                    _ => "String"
                };
                columns.Add($"{prop.Name} {chType}");
            }

            var createTableSql = $@"
                CREATE TABLE IF NOT EXISTS {tableName} (
                    {string.Join(",\n                    ", columns)}
                ) ENGINE = MergeTree()
                ORDER BY RequestTime";
            
            await using (var command = connection.CreateCommand())
            {
                command.CommandText = createTableSql;
                await command.ExecuteNonQueryAsync();
            }

            // Check for missing columns
            foreach (var prop in properties)
            {
                var chType = prop.PropertyType switch
                {
                    _ when prop.PropertyType == typeof(string) => "String",
                    _ when prop.PropertyType == typeof(int) => "Int32",
                    _ when prop.PropertyType == typeof(uint) => "UInt32",
                    _ when prop.PropertyType == typeof(long) => "Int64",
                    _ when prop.PropertyType == typeof(ulong) => "UInt64",
                    _ when prop.PropertyType == typeof(float) => "Float32",
                    _ when prop.PropertyType == typeof(double) => "Float64",
                    _ when prop.PropertyType == typeof(bool) => "UInt8",
                    _ when prop.PropertyType == typeof(DateTime) => "DateTime",
                    _ => "String"
                };

                var alterSql = $"ALTER TABLE {tableName} ADD COLUMN IF NOT EXISTS {prop.Name} {chType}";
                await using var command = connection.CreateCommand();
                command.CommandText = alterSql;
                await command.ExecuteNonQueryAsync();
            }

            logger.LogInformation("Clickhouse table '{TableName}' initialized and schema updated.", tableName);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to initialize Clickhouse table.");
        }
    }
}
