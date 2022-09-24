using Microsoft.Extensions.Logging;
using Slack.NetStandard;
using Slack.NetStandard.RequestHandler;

namespace BotZero.Common.Commands.Mapping;

public abstract class InteractiveCommandMapper : CommandMapper, ISlackRequestHandler<object?>
{
    private readonly ILogger<InteractiveCommandMapper> _logger;

    protected InteractiveCommandMapper(
        ILogger<InteractiveCommandMapper> logger,
        SlackWebApiClient client) : base(client)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected abstract bool CanHandle(SlackContext context);

    protected abstract Task Handle(SlackContext context);

    bool ISlackRequestHandler<object?>.CanHandle(SlackContext context)
    {
        // set context first to aide interaction
        Context = new ActionContext(context);

        try
        {
            return CanHandle(context);
        }
        catch (Exception ex)
        {
            HandleException(ex).RunSynchronously();
            throw;
        }
    }

    async Task<object?> ISlackRequestHandler<object?>.Handle(SlackContext context)
    {
        // set context first to aide interaction
        Context = new ActionContext(context);

        try
        {
            await Handle(context);
        }
        catch (Exception ex)
        {
            HandleException(ex).RunSynchronously();

            // don't throw when handling a request
            //throw
        }

        return null;
    }

    protected async Task HandleException(Exception ex)
    {
        _logger.LogError(ex, $"Error executing command: {Context.Message}");

        var err = ex.Message.Contains('\n') ? $"```{ex.Message}```" : $"`{ex.Message}`";

        var msg = $"Something went wrong! All I got was: {err} from `{GetType().FullName}` :scream:";

        if (Context.IsDirectMessage)
        {
            msg = $"<@{Context.UserId}>, " + msg;
        }

        await Client.Chat.PostMarkdownMessage(Context.ChannelId, msg);
    }
}
