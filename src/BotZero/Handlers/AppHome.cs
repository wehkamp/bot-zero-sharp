using BotZero.Commands;
using BotZero.Common;
using BotZero.Common.Templating;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;

namespace BotZero.Handlers;

public class AppHome : SlackRequestHandlerBase
{
    private readonly HelpCommand _helpCommand;
    private readonly IJsonTemplateGenerator _generator;

    public AppHome(
        HelpCommand helpCommand,
        IJsonTemplateGenerator generator,
        SlackWebApiClient client) : base(client)
    {
        _helpCommand = helpCommand ?? throw new ArgumentNullException(nameof(helpCommand));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
    }

    public override bool CanHandle(SlackContext context)
    {
        if (context.Interaction is BlockActionsPayload bap)
        {
            return bap.View?.CallbackId == "home";
        }
        else if (
            context.Event is EventCallback ecb &&
            ecb.Event is AppHomeOpened evt &&
            evt.Tab == "home")
        {
            return true;
        }

        return false;
    }

    public async override Task Handle(SlackContext context)
    {
        if (context.Interaction is BlockActionsPayload bap)
        {
            var query = bap.View.State.GetValue("query")?.Value;
            await Show(query, viewId: bap.View.ID);
        }
        else if (
            context.Event is EventCallback ecb &&
            ecb.Event is AppHomeOpened evt &&
            evt.Tab == "home")
        {
            string? query = evt.View.State.GetValue("query")?.Value;
            await Show(query, evt.View.ID, evt.User);
        }
    }

    protected async Task Show(string? query = null, string? viewId = null, string? user = null)
    {
        var data = new
        {
            query,
            help = await _helpCommand.QueryHelp(query)
        };

        var view = _generator.ParseWithManifestResource("BotZero.Handlers.Templates.AppHome.json.hbs", data);

        if (viewId != null)
        {
            await Client.MakeJsonCall("views.update", new { view_id = viewId, view });
        }
        else
        {
            await Client.MakeJsonCall("views.publish", new { user, view });
        }
    }
}
