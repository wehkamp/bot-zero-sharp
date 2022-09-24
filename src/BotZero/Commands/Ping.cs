using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using Slack.NetStandard;

[Help("ping - pings the bot", "ping n - pings the bot n times")]
public class PingCommand : CommandMapper
{
    public PingCommand(SlackWebApiClient client) : base(client)
    {
    }

    protected async Task Callback(int times = 1)
    {
        for (var i = 0; i < times; i++)
        {
            await Reply("Pong!");
        }
    }
}