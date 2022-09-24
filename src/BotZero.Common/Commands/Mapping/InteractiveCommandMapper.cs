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
        return CanHandle(context);
    }

    async Task<object?> ISlackRequestHandler<object?>.Handle(SlackContext context)
    {
        // set context first to aide interaction
        Context = new ActionContext(context);
        await Handle(context);
        return null;
    }
}
