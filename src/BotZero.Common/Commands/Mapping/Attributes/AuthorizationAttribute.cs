using static BotZero.Common.Slack.SlackProfileService;

namespace BotZero.Common.Commands.Mapping.Attributes;

/// <summary>
/// Only authorized persons will be able to access the feature.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizationAttribute : Attribute, IAuthorizationAttribute
{
    private readonly HashSet<string> _emails = new();

    public AuthorizationAttribute(params string[] emails)
    {
        emails
            .Select(x => x.ToLower())
            .ToList()
            .ForEach(email => _emails.Add(email));
    }

    public async virtual Task<bool> Authorize(SlackUser? user)
    {
        if (user == null) return false;
        if (String.IsNullOrWhiteSpace(user.Email)) return false;

        await Task.CompletedTask;

        return _emails.Contains(user.Email);
    }
}
