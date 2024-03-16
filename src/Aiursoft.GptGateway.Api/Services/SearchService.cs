using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Aiursoft.CSTools.Services;

namespace Aiursoft.GptGateway.Api.Services;

public class SearchService(IConfiguration configuration)
{
    private readonly string _searchApiKey = configuration["BingSearchAPIKey"]!;

    public virtual async Task<SearchResponse> DoSearch(string question, int count = 10)
    {
        var credential = new ApiKeyServiceClientCredentials(_searchApiKey);
        var client = new WebSearchClient(credential);
        client.SetPrivatePropertyValue("BaseUri", "{Endpoint}/v7.0");
        client.Endpoint = "https://api.bing.microsoft.com";
        var webData = await client.Web.SearchAsync(
            question,
            count: count);
        return webData;
    }
}
