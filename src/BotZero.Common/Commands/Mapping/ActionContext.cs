using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;
using static BotZero.Common.Slack.SlackProfileService;

namespace BotZero.Common.Commands.Mapping;

public class ActionContext : CommandContext
{
    public ActionContext() : base()
    {

    }

    public ActionContext(SlackContext context)
    {
        Message = "[Slack Interaction]";

        if (context.Interaction is BlockActionsPayload bap)
        {
            Timestamp = bap.Message?.Timestamp;
            ChannelId = bap.Channel?.ID ?? bap.User.ID;
            UserId = bap.User.ID;
            IsDirectMessage = ChannelId == UserId;
            User = new SlackUser
            {
                Email = bap.User.Email,
                IsBot = false,
                Name = bap.User.Name,
                UserId = bap.User.ID
            };

            var actionId = bap.Actions.FirstOrDefault()?.ActionId;
            if(actionId != null)
            {
                Message = "[Slack Interaction: " + actionId + "]";
            }
        }
    }

    /// <summary>
    /// The values that were extracted from the user input.
    /// </summary>
    public Dictionary<string, object?> Values { get; set; } = new Dictionary<string, object?>();

    /// <summary>
    /// The name of the action that is executed.
    /// </summary>
    public string ActionName { get; set; } = string.Empty;

    /// <summary>
    /// The command mapper that is executing the command. Can be used to
    /// access the Web API client (and features like send and reply).
    /// </summary>
    internal CommandMapper? CommandMapper { get; set; }

    /// <summary>
    /// The command that is exectuted.
    /// </summary>
    internal Command? Command { get; set; }
}
