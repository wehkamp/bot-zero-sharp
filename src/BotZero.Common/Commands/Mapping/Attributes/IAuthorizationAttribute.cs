using BotZero.Common.Slack;

namespace BotZero.Common.Commands.Mapping.Attributes
{
    public interface IAuthorizationAttribute
    {
        Task<bool> Authorize(SlackProfileService.SlackUser? user);
    }
}