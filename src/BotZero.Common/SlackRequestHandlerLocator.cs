using Slack.NetStandard.RequestHandler;

namespace BotZero.Common;

public class SlackRequestHandlerLocator
{
    public ISlackRequestHandler<object?>[] Handlers { get; }

    public SlackRequestHandlerLocator(ISlackRequestHandler<object?>[] handlers)
    {
        Handlers = handlers;
    }
}
