using BotZero.Common.Commands;
using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using Slack.NetStandard;

namespace BotZero.Commands;

/// <summary>
/// Creates a command that listens to help.
/// </summary>
[Help("help - Shows this help.")]
public class HelpCommand : CommandMapper
{
    public IList<ICommand> _commands { get; }

    public HelpCommand(SlackWebApiClient client, IList<ICommand> helpItems) : base(client)
    {
        _commands = helpItems ?? throw new ArgumentNullException(nameof(helpItems));
    }

    protected async Task Callback(string query = "")
    {
        var text = await QueryHelp(query);
        await Reply(text);
    }

    public async Task<string> QueryHelp(string? query)
    {
        var botUserId = await GetBotUserId();

        var auth = await Client.Auth.Test();
        var items = _commands
            .SelectMany(x => x.GetHelpLines())
            .Where(x => string.IsNullOrWhiteSpace(query) || x.Contains(query, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => $"• <@{botUserId}> {x}")
            .OrderBy(x => x)
            .ToList();

        var text = "";

        if (string.IsNullOrWhiteSpace(query))
        {
            text = $"I can do:\n\n" + string.Join("\n", items);
        }
        else if (items.Count != 0)
        {
            text = $"I can do these actions matching `{query}`:\n\n" + string.Join("\n", items);
        }
        else
        {
            text = $"Sorry, I can't do anything matching `{query}`.";
        }

        return text;
    }
}
