using Aiursoft.Canon;
using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PreRequest;

public class InjectPluginsMiddleware : IPreRequestMiddleware
{
    private readonly CanonPool _canonPool;
    private readonly IEnumerable<IPlugin> _plugins;

    public InjectPluginsMiddleware(
        CanonPool canonPool,
        IEnumerable<IPlugin> plugins)
    {
        _canonPool = canonPool;
        _plugins = plugins;
    }

    public async Task PreRequest(ConversationContext conv)
    {
        var pluginRanks = new List<(IPlugin plugin, int rank)>();
        foreach (var plugin in _plugins)
        {
            _canonPool.RegisterNewTaskToPool(async () =>
            {
                var usagePoint = await plugin.GetUsagePoint(conv.ModifiedInput);
                if (usagePoint > 0)
                {
                    pluginRanks.Add((plugin, usagePoint));
                }
            });
        }

        await _canonPool.RunAllTasksInPoolAsync(16);
        var bestPlugin = pluginRanks.OrderByDescending(t => t.rank).FirstOrDefault();
        if (bestPlugin.plugin == null)
        {
            return;
        }
        
        conv.ToolsUsed.Add(bestPlugin.plugin.PluginName);
        var message = await bestPlugin.plugin.GetPluginAppendedMessage(conv);
        
        // TODO: May not replace but insert. Multiple plugins may be used at the same time.
        // Replace the last message.
        conv.ModifiedInput.Messages[^1] = new MessagesItem
        {
            Role = "user",
            Content = message,
        };
    }
}