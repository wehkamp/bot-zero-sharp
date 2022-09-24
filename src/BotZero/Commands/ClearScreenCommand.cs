using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using Slack.NetStandard;

namespace BotZero.Commands
{
    [Command("clear")]
    public class ClearScreenCommand : CommandMapper
    {
        public ClearScreenCommand(SlackWebApiClient client) : base(client)
        {
        }

        [Action]
        public async Task Screen()
        {
            for(var i = 0; i < 48; i++)
            {
                await Send("   ");
            }
        }
    }
}
