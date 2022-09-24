using MarkdownDeep;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi.CallbackEvents;
using System.Text.RegularExpressions;
using static BotZero.Common.Slack.SlackProfileService;

namespace BotZero.Common.Commands;

/// <summary>
/// The context of the direct message or mention.
/// </summary>
public class CommandContext
{
    private static readonly Markdown _markdownRemover = new()
    {
        SummaryLength = -1
    };

    public CommandContext()
    {
    }

    public CommandContext(MessageCallbackEvent message, SlackUser? user)
    {
        // remove markdown and whitespaces
        Message = _markdownRemover.Transform(message.Text).Trim();

        UserId = message.User;
        ChannelId = message.Channel;
        Timestamp = message.Timestamp;
        IsDirectMessage = true;
        User = user;
    }

    public CommandContext(AppMention mention, SlackUser? user)
    {
        // remove mention
        Message = Regex.Replace(mention.Text, "^<[^ >]+>\\s+", "");

        // remove markdown and whitespaces
        Message = _markdownRemover.Transform(Message).Trim();

        UserId = mention.User;
        ChannelId = mention.Channel;
        Timestamp = mention.Timestamp;
        IsDirectMessage = false;
        User = user;
    }

    /// <summary>
    /// Indicates the context is from a direct message.
    /// </summary>
    public bool IsDirectMessage { get; set; } = false;

    /// <summary>
    /// The message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The user that sent the message.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The channel from which the message was sent.
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the message that was sent.
    /// </summary>
    public Timestamp? Timestamp { get; set; }

    /// <summary>
    /// The user.
    /// </summary>
    public SlackUser? User { get; set; }
}
