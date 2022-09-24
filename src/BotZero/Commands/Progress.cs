using BotZero.Common.Commands;
using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Slack.Common.Commands;
using Slack.NetStandard;
using System.Text.RegularExpressions;

namespace BotZero.Commands;

[Help("progress - Shows how to build a message that's updatable showing progress.")]
public class Progress : RegexCommandBase
{
    private readonly string[] _steps = new string[] {
      "Preparing environment...",
      "Adding some love...",
      "Contacting support...",
      "Building relationships...",
      "Shipping code...",
      "Testing. Testing. Testing...",
      "Testing some more...",
      "Preparing for launch...",
      "Validating details...",
      "Organizing a pre-launch party...",
      "Done!"
    };

    public Progress(SlackWebApiClient client) : base(client, new Regex("^progress$", RegexOptions.IgnoreCase))
    {
    }

    protected async override Task Callback(Match m, CommandContext context)
    {
        var msg = CreateUpdatableMessage(context);

        msg.Send("Showing an example of a progress indicator... *0%*");

        // careful with flooding the Slack API with too many
        // messages. Consider that a single command might be
        // executed by multiple users.
        int ms = 750;

        double i = 1;
        while (true)
        {
            int step = (int)Math.Floor(i / (_steps.Length - 1));
            msg.Send($"{_steps[step]} *{i}*%");

            i += 3;

            if (i > 100)
            {
                break;
            }
            await Task.Delay(ms);
        }

        await msg.WaitForDelivery();
    }
}
