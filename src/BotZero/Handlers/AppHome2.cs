using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Common.Slack;
using BotZero.Common.Templating;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Slack.NetStandard;
using Slack.NetStandard.EventsApi;
using Slack.NetStandard.EventsApi.CallbackEvents;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;
using System.Web;

[Help("ktt search `topic` - searches KeesTalksTech for the topic.")]
public class AppHome2 : InteractiveCommandMapper
{
    private readonly IJsonTemplateGenerator _generator;

    public AppHome2(IJsonTemplateGenerator generator, ILogger<InteractiveCommandMapper> logger, SlackWebApiClient client) : base(logger, client)
    {
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
            items = Query(query)
        };

        var obj = _generator.ParseWithManifestResourceToObject("BotZero.Handlers.Templates.AppHome2.json.hbs", data);
        var isNew = view != null;
        var action = isNew ? "views.publish" : "views.update";

        await Client.MakeJsonCall(
            action,
            new { user_id = userId, view_id = view?.ID, view = obj });
    }
        

    protected object[] Query(string? query, int? maxSize = null)
    {
        var url = @"https://keestalkstech.com/?s=" + HttpUtility.UrlEncode(query ?? "");
        var web = new HtmlWeb();
        var htmlDoc = web.Load(url);
        var read = (HtmlNode node) => HttpUtility.HtmlDecode(node.InnerText).Trim();

        var notFound = htmlDoc.QuerySelector("#post-0") != null;
        if (notFound) return Array.Empty<string>();

        return htmlDoc.QuerySelectorAll("article").Select(x => new
        {
            title = read(x.QuerySelector("h2")),
            url = x.QuerySelector(".text > a").Attributes["href"].Value,
            excerpt = read(x.QuerySelector(".excerpt_part")),
            image = x.QuerySelector("img").Attributes["src"].Value,
            categories = x.QuerySelectorAll(".categories a").Select(read).ToArray(),
        }).Take(maxSize ?? int.MaxValue).ToArray();
    }

    [Action("")]
    protected async Task Search(string topic = "")
    {
        var data = new
        {
            topic,
            items = Query(topic, 3),
            skipHeader = true,
        };

        var view = _generator.ParseWithManifestResourceToObject("BotZero.Handlers.Templates.AppHome2.json.hbs", data);

        await Client.MakeJsonCall("chat.postMessage", new
        {
            channel = Context.ChannelId,
            text = topic == null ? "Blogs from KeesTalksTech." : "Search result KeesTalksTech for: " + topic,
            unfurl_links = false,
            unfurl_media = false,
            parse = "none",
            blocks = view?.blocks
        });
    }
}
