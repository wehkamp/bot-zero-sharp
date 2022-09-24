using BotZero.Commands;
using BotZero.Common.Commands.Mapping;
using BotZero.Common.Slack;
using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.RequestHandler;
using System.Text.RegularExpressions;

namespace BotZero.Common.Commands;

public sealed class CommandHandler : ISlackRequestHandler<object?>
{
    private static string? _botUserId = null;

    private readonly SlackWebApiClient _client;
    private readonly SlackProfileService _profileService;
    private readonly ILogger<CommandHandler> _logger;
    private readonly List<ICommand> _commands = new();

    public CommandHandler(
        SlackWebApiClient client,
        SlackProfileService profileService,
        ILogger<CommandHandler> logger,
        ICommand[] commands)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        HelpCommand = new HelpCommand(client, _commands);

        _commands.AddRange(commands);
        _commands.Add(HelpCommand);
    }

    public HelpCommand HelpCommand { get;}

    /// <summary>
    /// Gets the UserId of the bot user.
    /// </summary>
    /// <returns>The UserId.</returns>
    private async Task<string> GetBotUserId()
    {
        if (string.IsNullOrEmpty(_botUserId))
        {
            var auth = await _client.Auth.Test();
            auth.EnsureSuccess();
            _botUserId = auth.UserId;
        }

        return _botUserId;
    }

    bool ISlackRequestHandler<object?>.CanHandle(SlackContext context)
    {
        if (context.Event is EventCallback evc)
        {
            if (evc.Event is MessageCallbackEvent message)
            {
                var skip = message is MessageChanged || message is MessageDeleted;
                if (!skip)
                {
                    // remove mention of bot from direct message
                    if (message.Text.StartsWith("<@"))
                    {
                        var botUserId = GetBotUserId().Result;
                        message.Text = Regex.Replace(message.Text, $"^<@{Regex.Escape(botUserId)}>\\s+", "");
                    }

                    var user = _profileService.GetUser(message.User).Result;
                    var ctx = new CommandContext(message, user);
                    var task = ProcessCommand(ctx);
                    return task.Result;
                }
            }
            else if (evc.Event is AppMention mention)
            {
                var user = _profileService.GetUser(mention.User).Result;
                var ctx = new CommandContext(mention, user);
                var task = ProcessCommand(ctx);
                return task.Result;
            }
        }

        foreach (var command in _commands)
        {
            if (command is ISlackRequestHandler<object?> srh)
            {
                try
                {
                    if (srh.CanHandle(context))
                    {
                        var task = srh.Handle(context);
                        task.Wait();
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    var ac = new ActionContext(context);
                    var task = HandleError(ac, command, ex);
                    task.Wait();
                    return true;
                }
            }
        }

        return false;
    }

    async Task<object?> ISlackRequestHandler<object?>.Handle(SlackContext context)
    {
        await Task.CompletedTask;
        return null;
    }

    private async Task<bool> ProcessCommand(CommandContext context)
    {
        foreach (var command in _commands)
        {
            try
            {
                if (await command.Process(context))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                await HandleError(context, command, ex);
                return true;
            }
        }

        return false;
    }

    private async Task HandleError(CommandContext context, ICommand command, Exception ex)
    {
        _logger.LogError(ex, $"Error executing command: {context.Message}");

        var err = ex.Message.Contains('\n') ? $"```{ex.Message}```" : $"`{ex.Message}`";

        var msg = $"Something went wrong! All I got was: {err} from `{command.GetType().FullName}` :scream:";

        if (context.IsDirectMessage)
        {
            msg = $"<@{context.UserId}>, " + msg;
        }

        await _client.Chat.PostMarkdownMessage(context.ChannelId, msg);
    }
}
