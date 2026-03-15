using Aiursoft.GptGateway.Entities;
using Aiursoft.GptGateway.Models.Configuration;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Data;

public class ClickhouseDbContext : IAsyncDisposable, IDisposable
{
    private ClickHouseConnection? _connection;
    private readonly ClickhouseOptions _config;
    private readonly ILogger<ClickhouseDbContext> _logger;

    public ClickhouseSet<RequestLog> RequestLogs { get; }
    public bool Enabled => _config.Enabled;

    public ClickhouseDbContext(IOptionsMonitor<ClickhouseOptions> options, ILogger<ClickhouseDbContext> logger)
    {
        _config = options.CurrentValue;
        _logger = logger;
        RequestLogs = new ClickhouseSet<RequestLog>(GetConnection, "RequestLogs", log => new object[] 
        {
            log.IP,
            log.ConversationMessageCount,
            log.LastQuestion,
            log.Model,
            log.Success ? 1 : 0,
            log.Duration,
            log.Thinking,
            log.Answer,
            log.RequestTime,
            log.Method,
            log.Path,
            log.StatusCode,
            log.UserAgent,
            log.TraceId
        });
    }

    private async Task<ClickHouseConnection> GetConnection()
    {
        if (_connection == null)
        {
            _connection = new ClickHouseConnection(_config.ConnectionString);
            await _connection.OpenAsync();
        }
        return _connection;
    }

    public async Task SaveChangesAsync()
    {
        if (!_config.Enabled)
        {
            return;
        }

        try
        {
            await RequestLogs.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save logs to Clickhouse.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
