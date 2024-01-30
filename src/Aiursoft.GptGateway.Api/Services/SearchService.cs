using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;

namespace Aiursoft.GptGateway.Api.Services;

public class SearchService
{
    private readonly string _searchApiKey;
    public SearchService(
        IConfiguration configuration)
    {
         _searchApiKey = configuration["BingSearchAPIKey"]!;
    }

    public virtual async Task<SearchResponse> DoSearch(string question, string lang = "", int page = 1)
    {
        var credential = new ApiKeyServiceClientCredentials(_searchApiKey);
        var client = new WebSearchClient(credential);
        var webData = await client.Web.SearchAsync(
            question,
            count: 10,
            offset: (page - 1) * 10,
            market: lang,
            setLang: lang);
        return webData;
    }
}