using Aiursoft.GptGateway.Api.Services;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Microsoft.Extensions.Configuration;

namespace Aiursoft.GptGateway.Tests.Services;

public class MockSearchService(IConfiguration configuration) : SearchService(configuration)
{
    public override Task<SearchResponse> DoSearch(string question, int count = 10)
    {
        return Task.FromResult(new SearchResponse());
    }
}