using ClickHouse.Client.ADO;
using ClickHouse.Client.Copy;

namespace Aiursoft.GptGateway.Data;

public class ClickhouseSet<T>(
    Func<Task<ClickHouseConnection>> connectionFactory, 
    string tableName, 
    Func<T, object[]> mapper) where T : class
{
    internal readonly List<T> _local = new();

    public void Add(T entity)
    {
        _local.Add(entity);
    }

    public async Task SaveChangesAsync()
    {
        if (_local.Count == 0)
        {
            return;
        }

        var connection = await connectionFactory();
        using var bulkCopy = new ClickHouseBulkCopy(connection)
        {
            DestinationTableName = tableName,
            BatchSize = _local.Count
        };

        await bulkCopy.WriteToServerAsync(_local.Select(mapper));
        _local.Clear();
    }
}
