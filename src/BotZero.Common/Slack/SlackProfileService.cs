using Microsoft.Extensions.Caching.Memory;
using Slack.NetStandard;

namespace BotZero.Common.Slack
{
    public class SlackProfileService
    {
        private readonly IMemoryCache _cache;
        private readonly SlackWebApiClient _client;

        public SlackProfileService(IMemoryCache cache, SlackWebApiClient client)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<SlackUser?> GetUser(string userId)
        {
            return _cache.GetOrCreateAsync("user." + userId, async (key) =>
            {
                var result = await _client.Users.Info(userId);

                if (result.User == null) return null;

                return new SlackUser
                {
                    UserId = userId,
                    Email = result.User.Profile.Email,
                    Name = result.User.RealName,
                    ProfilePicture = result.User.Profile.Image192,
                    IsBot = result.User.IsBot == true
                };
            });
        }

        public class SlackUser
        {
            public string UserId { get; set; } = string.Empty;

            public string ProfilePicture { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            public bool IsBot { get; set; }
        }
    }
}
