namespace BotZero.Common.Slack;

[Serializable]
public class SlackApiException : Exception
{
    public SlackApiException()
    {
    }

    public SlackApiException(string? message, string method, params string[] messages) : base(CreateMessage(message, method, messages))
    {
    }

    private static string CreateMessage(string? message, string method, string[] messages)
    {
        var final = method + ": " + message;

        if (messages != null && messages.Length > 0)
        {
            final += "\n" + string.Join("\n", messages);
        }

        return final;
    }
}
