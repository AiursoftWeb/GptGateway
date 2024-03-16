using Aiursoft.Canon;
using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.PreRequest;

public class InjectPluginsMiddleware(
    CanonPool canonPool,
    IEnumerable<IPlugin> plugins) : IPreRequestMiddleware
{
    public async Task PreRequest(ConversationContext conv)
    {
        var pluginRanks = new List<(IPlugin plugin, int rank)>();
        foreach (var plugin in plugins)
        {
            canonPool.RegisterNewTaskToPool(async () =>
            {
                var usagePoint = await plugin.GetUsagePoint(conv.ModifiedInput);
                if (usagePoint > 0)
                {
                    pluginRanks.Add((plugin, usagePoint));
                }
            });
        }

        await canonPool.RunAllTasksInPoolAsync(16);
        var bestPlugin = pluginRanks.OrderByDescending(t => t.rank).FirstOrDefault();
        if (bestPlugin.plugin == null)
        {
            return;
        }
        
        conv.ToolsUsed.Add(bestPlugin.plugin);
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