using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Slack.NetStandard;
using System.Dynamic;

namespace BotZero.Commands;

[Help(
    "norris - shows a Chuck Norris quote.",
    "norris impersonate `first-name` `last-name` - shows a Chuck Norris quote for the given name.",
    "norris nr `number` - shows the numbered quote.")]
public class Norris : CommandMapper
{
    private const string _idQuoteUrl = "http://api.icndb.com/jokes";
    private const string _randomQuoteUrl = "http://api.icndb.com/jokes/random?escape=javascript&exlude=%5Bexplicit%5D";

    private readonly HttpClient _httpClient;

    public Norris(HttpClient httpClient, SlackWebApiClient client) : base(client)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    [Action("")]
    protected async Task Joke()
    {
        var joke = await GetJoke(_randomQuoteUrl);
        await Reply(joke);
    }

    [Action("")]
    protected async Task JokeById(int id)
    {
        var url = $"{_idQuoteUrl}/{id}/?escape=javascript";
        try
        {
            var joke = await GetJoke(url);
            await Reply(joke);
        }
        catch
        {
            await Reply("Sorry, that on doesn't exist.");
        }
    }

    [Action]
    protected async Task Impersonate(string firstName, string lastName)
    {
        var q = new Dictionary<string, string>
        {
            { "firstName", firstName },
            { "lastName" , lastName }
        };

        var url = new Uri(QueryHelpers.AddQueryString(_randomQuoteUrl, q));
        var joke = await GetJoke(url.ToString());
        await Reply(joke);
    }

    private async Task<string> GetJoke(string url)
    {
        var json = await _httpClient.GetStringAsync(url);
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        dynamic resp = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        if (resp?.type != "success")
        {
            throw new Exception(resp?.value as string ?? "Invalid query.");
        }

        return resp.value.joke;
    }
}
