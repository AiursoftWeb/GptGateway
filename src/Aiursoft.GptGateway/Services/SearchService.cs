using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;
using Aiursoft.CSTools.Services;

namespace Aiursoft.GptGateway.Services;

public class SearchService(
    ILogger<SearchService> logger,
    IConfiguration configuration)
{
    private readonly string _searchApiKey = configuration["BingSearchAPIKey"]!;

    public virtual async Task<SearchResponse> DoSearch(string question, int count = 10, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Searching for {question} with count {count}", question, count);
        var credential = new ApiKeyServiceClientCredentials(_searchApiKey);
        var client = new WebSearchClient(credential);
        client.SetPrivatePropertyValue("BaseUri", "{Endpoint}/v7.0");
        client.Endpoint = "https://api.bing.microsoft.com";
        var webData = await client.Web.SearchAsync(
            question,
            count: count,
            cancellationToken: cancellationToken);
        return webData;
    }
}
