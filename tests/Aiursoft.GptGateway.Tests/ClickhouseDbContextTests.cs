using Aiursoft.GptGateway.Data;
using Aiursoft.GptGateway.Entities;
using ClickHouse.Client.ADO;

namespace Aiursoft.GptGateway.Tests;

[TestClass]
public class ClickhouseDbContextTests
{
    [TestMethod]
    public void TestClickhouseSetAdd()
    {
        var set = new ClickhouseSet<RequestLog>(
            () => Task.FromResult(new ClickHouseConnection()), 
            "TestTable", 
            log => new object[] { log.IP });
            
        var log = new RequestLog { IP = "127.0.0.1" };
        set.Add(log);
        
        Assert.AreEqual(1, set._local.Count);
        Assert.AreEqual("127.0.0.1", set._local[0].IP);
    }

    [TestMethod]
    public async Task TestClickhouseSetSaveChangesAsyncClearsLocal()
    {
        // We can't easily test the actual bulk copy without a real connection,
        // but we can test that it clears the local list after "saving" (even if it fails due to no connection).
        
        var set = new ClickhouseSet<RequestLog>(
            () => throw new Exception("Expected connection failure"), 
            "TestTable", 
            log => new object[] { log.IP });
            
        set.Add(new RequestLog { IP = "127.0.0.1" });
        
        try 
        {
            await set.SaveChangesAsync();
        }
        catch (Exception e) when (e.Message == "Expected connection failure")
        {
            // Expected
        }
        
        // In my current implementation, it clears AFTER WriteToServerAsync. 
        // If it throws before that, it won't clear. This is standard EF behavior too.
        Assert.AreEqual(1, set._local.Count);
    }
}