using BotZero.Common.Slack;
using Slack.NetStandard;
using Slack.NetStandard.RequestHandler;

namespace BotZero.Common;

public abstract class SlackRequestHandlerBase : ISlackRequestHandler<object?>
{
    private string? _botUserId;

    public SlackRequestHandlerBase(SlackWebApiClient client)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public SlackWebApiClient Client { get; }

    protected abstract bool CanHandle(SlackContext context);

    protected abstract Task Handle(SlackContext context);

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

    bool ISlackRequestHandler<object?>.CanHandle(SlackContext context)
    {
        return CanHandle(context);
    }

    async Task<object?> ISlackRequestHandler<object?>.Handle(SlackContext context)
    {
        await Handle(context);
        return null;
    }
}
