using System.Reflection;
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

    public virtual async Task<SearchResponse> DoSearch(string question, int page = 1)
    {
        var credential = new ApiKeyServiceClientCredentials(_searchApiKey);
        var client = new WebSearchClient(credential)
        {
        };
        client.SetPrivatePropertyValue("BaseUri", "{Endpoint}/v7.0");
        client.Endpoint = "https://api.bing.microsoft.com";
        var webData = await client.Web.SearchAsync(
            question,
            count: 10);
        return webData;
    }
}

public static class ReflectionExtensions
{
/// <summary>
/// Sets a _private_ Property Value from a given Object. Uses Reflection.
/// Throws a ArgumentOutOfRangeException if the Property is not found.
/// </summary>
/// <typeparam name="T">Type of the Property</typeparam>
/// <param name="obj">Object from where the Property Value is set</param>
/// <param name="propName">Propertyname as string.</param>
/// <param name="val">Value to set.</param>
/// <returns>PropertyValue</returns>
public static void SetPrivatePropertyValue<T>(this object obj, string propName, T val)
{
    var t = obj.GetType();
    if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
        throw new ArgumentOutOfRangeException(nameof(propName),
            $"Property {propName} was not found in Type {obj.GetType().FullName}");
    t.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object[] { val });
}
}