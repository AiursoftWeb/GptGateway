using Aiursoft.GptGateway.Services;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aiursoft.GptGateway.Tests.Services;

public class MockSearchService(ILogger<SearchService> logger, IConfiguration configuration) : SearchService(logger, configuration)
{
    public override Task<SearchResponse> DoSearch(string question, int count = 10, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new SearchResponse());
    }
}
