using Aiursoft.GptGateway.Entities;
using Aiursoft.GptGateway.Models.Configuration;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.Options;

namespace Aiursoft.GptGateway.Data;

public class ClickhouseDbContext : IAsyncDisposable, IDisposable
{
    private ClickHouseConnection? _connection;
    private readonly ClickhouseOptions _config;

    public ClickhouseSet<RequestLog> RequestLogs { get; }

    public ClickhouseDbContext(IOptionsMonitor<ClickhouseOptions> options)
    {
        _config = options.CurrentValue;
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
            log.RequestTime
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
        await RequestLogs.SaveChangesAsync();
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
