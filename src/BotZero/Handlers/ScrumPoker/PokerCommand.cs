using BotZero.Common.Commands.Mapping;
using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Common.Commands.Mapping.Parameters;
using BotZero.Common.Slack;
using BotZero.Common.Templating;
using Newtonsoft.Json;
using Slack.NetStandard;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;
using System.Collections.Concurrent;

namespace BotZero.Handlers.ScrumPoker;

[Help("poker - start a planning poker", "poker {topic} - start a planning poker on the specified topic.")]
public class PokerCommand : InteractiveCommandMapper
{
    private readonly static ConcurrentDictionary<string, Vote> _votes = new ConcurrentDictionary<string, Vote>();
    private readonly IJsonTemplateGenerator _generator;

    public PokerCommand(IJsonTemplateGenerator generator, ILogger<InteractiveCommandMapper> logger, SlackWebApiClient client) : base(logger, client)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
    }

    protected async Task Callback([RestParameter(true)] string topic = "")
    {
        var vote = new Vote
        {
            ID = Guid.NewGuid().ToString(),
            Topic = topic
        };

        vote.RegisterVote(Context.User?.UserId!, Context.User?.Name!, null);
        if (!string.IsNullOrEmpty(topic))
        {
            vote.Stage = Vote.Stages.Voting;
        }

        await SendMessage(vote, true);
    }

    protected override bool CanHandle(SlackContext context)
    {
        if (context.Interaction is BlockActionsPayload bap)
        {
            return bap.Message?.Metadata?.EventType == "poker";
        }

        return false;
    }

    protected override async Task Handle(SlackContext context)
    {
        var bap = (context.Interaction as BlockActionsPayload)!;
        var action = bap.Actions.First();
        var vote = GetVote(bap);

        // if a non-player interacts in any way, add it as a player
        if (!vote.Players.ContainsKey(Context.UserId))
        {
            vote.RegisterVote(Context.UserId, Context!.User!.Name, null);
        }

        switch (action?.ActionId)
        {
            case "start":
                vote.Topic = bap.State.GetValue("topic")?.Value;
                if (!string.IsNullOrEmpty(vote.Topic))
                {
                    vote.Stage = Vote.Stages.Voting;
                }
                break;
            case "reveal":
                vote.Stage = Vote.Stages.Consensus;
                break;
            case "poker_again":
                vote.Reset();
                vote.Stage = Vote.Stages.Voting;
                break;
            case "finish":
                vote.Stage = Vote.Stages.Finished;
                break;
            case "cancel":
                vote.Stage = Vote.Stages.Cancelled;
                break;
            case "stop":
                vote.Stage = Vote.Stages.Stopped;
                break;
            default:
                switch (action?.BlockId)
                {
                    case "player-vote":
                        vote.RegisterVote(bap.User.ID, bap.User.Name, action.Value);
                        break;
                    case "consensus-vote":
                        vote.Stage = Vote.Stages.Results;
                        vote.Points = action.Value;
                        break;
                }
                break;
        }

        await SendMessage(vote, false);
    }

    private static Vote GetVote(BlockActionsPayload bap)
    {
        var obj = (dynamic)bap.Message.Metadata.EventPayload;
        var id = obj.ID.ToString();

        if (!_votes.ContainsKey(id))
        {
            var vote = new Vote
            {
                ID = id,
                Points = obj.Points,
                Topic = obj.Topic,
                Stage = obj.Stage
            };

            var players = obj.Players.ToObject<Dictionary<string, dynamic>>();

            foreach (var key in players.Keys)
            {
                var p = players[key];
                vote.Players[key] = new Vote.Player
                {
                    Name = p.Name,
                    Vote = p.Vote
                };
            }

            _votes.TryAdd(id, vote);
        }

        return _votes[id];
    }

    private async Task SendMessage(Vote vote, bool isNew)
    {
        var template = _generator.ParseWithManifestResourceToObject("BotZero.Handlers.Templates.Poker.json.hbs", vote);
        var action = isNew ? "chat.postMessage" : "chat.update";
        var text = isNew ? "A new poker round begins..." : "The poker round was updated...";
        var ts = isNew ? null : Context.Timestamp?.ToString();
        var data = new
        {
            channel = Context.ChannelId,
            metadata = new
            {
                event_type = "poker",
                event_payload = JsonConvert.SerializeObject(vote)
            },
            text,
            template?.blocks,
            ts
        };

        await Client.MakeJsonCall(action, data);
    }
}

