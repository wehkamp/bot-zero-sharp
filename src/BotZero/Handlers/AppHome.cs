using BotZero.Commands;
using BotZero.Common;
using BotZero.Common.Slack;
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

    protected override bool CanHandle(SlackContext context)
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

    protected async override Task Handle(SlackContext context)
    {
        var bap = context.Interaction as BlockActionsPayload;
        var evt = (context.Event as EventCallback)?.Event as AppHomeOpened;

        var view = bap?.View ?? evt?.View;
        var userId = bap?.User.ID ?? evt?.User;
        var query = view?.State.GetValue("query")?.Value;

        var data = new
        {
            query,
            help = await _helpCommand.QueryHelp(query)
        };

        var obj = _generator.ParseWithManifestResourceToObject("BotZero.Handlers.Templates.AppHome.json.hbs", data);
        var isNew = view == null;
        var action = isNew ? "views.publish" : "views.update";

        await Client.MakeJsonCall(
            action, 
            new { user_id = userId, view_id = view?.ID, view = obj });
    }
}
