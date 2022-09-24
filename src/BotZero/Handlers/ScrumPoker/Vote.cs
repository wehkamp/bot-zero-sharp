namespace BotZero.Handlers.ScrumPoker;

public class Vote
{
    public string ID { get; set; } = string.Empty;

    public string? Topic { get; set; }

    public string? Points { get; set; }
    
    public Stages Stage { get; set; } = Stages.Start;

    public Dictionary<string, Player> Players { get; } = new Dictionary<string, Player>();

    public string Board
    {
        get
        {
            var padding = Players.Values.Max(x => x.Name.Length) + 2;

            var lines = Players
                .Values
                .OrderBy(x => x.Name)
                .Select(x =>
                    x.Name.PadRight(padding) + (
                        Stage == Stages.Consensus ? x.Vote ?? "" :
                        !string.IsNullOrWhiteSpace(x.Vote) ? "✔" : "…")
                );

            return string.Join("\n", lines);
        }
    }

    public void RegisterVote(string userId, string userName, string? vote)
    {
        if (!Players.ContainsKey(userId))
        {
            Players[userId] = new Player
            {
                Name = userName,
                Vote = vote
            };
        }
        else
        {
            Players[userId].Vote = vote;
        }
    }

    public void Reset()
    {
        Stage = Stages.Voting;
        Players.Values.ToList().ForEach(x => x.Vote = null);
    }

    public enum Stages
    {
        Start,
        Voting,
        Consensus,
        Results,
        Cancelled,
        Stopped,
        Finished
    }

    public class Player
    {
        public string Name { get; set; } = string.Empty;
        public string? Vote { get; set; }
    }
}

