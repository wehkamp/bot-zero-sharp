using BotZero.Common;
using BotZero.Common.Commands;
using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Common.Messaging;
using BotZero.Common.Slack;
using Slack.NetStandard;
using System.Reflection;

namespace BotZero.Slack.Common.Commands;

/// <summary>
/// Base class for a command.
/// </summary>
public abstract class CommandBase : ICommand
{
    private string? _botUserId;

    /// <summary>
    /// Client to interact with the Slack Web API.
    /// </summary>
    protected SlackWebApiClient Client { get; }

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="client">The Slack Web API.</param>
    protected CommandBase(SlackWebApiClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    /// <summary>
    /// Gets the UserId of the bot user.
    /// </summary>
    /// <returns>The UserId.</returns>
    protected async Task<string> GetBotUserId()
    {
        if (string.IsNullOrEmpty(_botUserId))
        {
            var auth = await Client.Auth.Test();
            auth.EnsureSuccess();
            _botUserId = auth.UserId;
        }

        return _botUserId;
    }

    /// <summary>
    /// Echos a message from the bot to the channel.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="msg">The message.</param>
    /// <returns>The ID of the message.</returns>
    public Task<Timestamp> Send(CommandContext context, string msg)
    {
        return Client.Chat.PostMarkdownMessage(context.ChannelId, msg);
    }

    /// <summary>
    /// Echos a reply to a user. If the context is not a direct message, the user will be mentioned.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="msg">The messasge.</param>
    /// <returns>The ID of the message.</returns>
    public Task Reply(CommandContext context, string msg)
    {
        if (!context.IsDirectMessage)
        {
            msg = $"<@{context.UserId}>, {msg}";
        }

        return Send(context, msg);
    }

    /// <summary>
    /// Creates an updatable message which can be updated multiple times.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The message.</returns>
    public UpdatableMessage CreateUpdatableMessage(CommandContext context)
    {
        return new UpdatableMessage(Client, context.ChannelId);
    }

    /// <summary>
    /// Resolves the help lines from the <c>HelpAttribute</c> attribute.
    /// </summary>
    /// <returns>The help lines.</returns>
    public virtual string[] GetHelpLines()
    {
        var help = GetType()
            .GetCustomAttribute<HelpAttribute>();

        if (help == null) return Array.Empty<string>();

        return help.Help;
    }

    /// <summary>
    /// Processes a direct message or mention.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns><c>True</c> if the message was processed successful.</returns>
    public abstract Task<bool> Process(CommandContext context);
}
