using Aiursoft.GptGateway.Api.Services;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Microsoft.Extensions.Configuration;

namespace Aiursoft.GptGateway.Tests.Services;

public class MockSearchService : SearchService
{
    public MockSearchService(IConfiguration configuration) : base(configuration)
    {
    }
    
    public override Task<SearchResponse> DoSearch(string question, int count = 10)
    {
        return Task.FromResult(new SearchResponse());
    }
}