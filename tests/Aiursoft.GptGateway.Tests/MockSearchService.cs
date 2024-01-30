using Aiursoft.GptGateway.Api.Services;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Microsoft.Extensions.Configuration;

namespace Aiursoft.GptGateway.Tests;

public class MockSearchService : SearchService
{
    public MockSearchService(IConfiguration configuration) : base(configuration)
    {
    }
    
    public override Task<SearchResponse> DoSearch(string question, int page = 1)
    {
        return Task.FromResult(new SearchResponse());
    }
}