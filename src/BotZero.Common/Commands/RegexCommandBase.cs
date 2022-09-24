using BotZero.Common.Commands;
using BotZero.Common.Messaging;
using Slack.NetStandard;
using System.Text.RegularExpressions;

namespace BotZero.Slack.Common.Commands;

/// <summary>
/// Command that responds to user input based on a regular expression.
/// </summary>
public abstract class RegexCommandBase : CommandBase
{
    private readonly Regex _command;

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="client">The Slack Web API client.</param>
    /// <param name="command">The regular expression.</param>
    protected RegexCommandBase(SlackWebApiClient client, Regex command) : base(client)
    {
        _command = command ?? throw new ArgumentNullException(nameof(command));
    }

    public override async Task<bool> Process(CommandContext context)
    {
        var match = _command.Match(context.Message);
        if (match.Success)
        {
            Context = context;
            await Callback(match, context);
            return true;
        }

        return false;
    }

    /// <summary>
    /// The current context that is executing the command.
    /// </summary>
    protected CommandContext Context { get; private set; } = new CommandContext();

    /// <summary>
    /// Exectuted when the regular expression matches the user input.
    /// </summary>
    /// <param name="m">The match.</param>
    /// <param name="context">The context.</param>
    protected abstract Task Callback(Match m, CommandContext context);

    /// <summary>
    /// Echos a message from the bot to the channel.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="msg">The message.</param>
    /// <returns>The ID of the message.</returns>
    protected Task<Timestamp> Send(string msg)
    {
        return Send(Context, msg);
    }

    /// <summary>
    /// Echos a reply to a user. If the context is not a direct message, the user will be mentioned.
    /// </summary>
    /// <param name="msg">The messasge.</param>
    /// <returns>The ID of the message.</returns>
    protected Task Reply(string msg)
    {
        return Reply(Context, msg);
    }

    /// <summary>
    /// Creates an updatable message which can be updated multiple times.
    /// </summary>
    /// <returns>The message.</returns>
    protected UpdatableMessage CreateUpdatableMessage()
    {
        return CreateUpdatableMessage(Context);
    }
}
