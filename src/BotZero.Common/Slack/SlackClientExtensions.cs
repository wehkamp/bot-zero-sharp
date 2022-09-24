using Slack.NetStandard;
using Slack.NetStandard.Objects;
using Slack.NetStandard.WebApi;
using Slack.NetStandard.WebApi.Chat;
using Slack.NetStandard.WebApi.View;
using System.Runtime.CompilerServices;

namespace BotZero.Common.Slack
{
    public static class SlackClientExtensions
    {
        /// <summary>
        /// When the Slack API returns a response, it might contain an error. When it does,
        /// an error is thrown.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="caller">The caller method.</param>
        public static void EnsureSuccess(this WebApiResponseBase response, [CallerMemberName] string caller = "")
        {
            if (response != null && !string.IsNullOrEmpty(response.Error))
            {
                var messages = new List<string>();

                if (response.Errors != null)
                {
                    messages.AddRange(response.Errors.Select(e => e.Message));
                }

                if (response is ViewResponse vr && vr.ResponseMetadata != null)
                {
                    messages.AddRange(vr.ResponseMetadata.Messages);
                }

                throw new SlackApiException(
                    caller,
                    response.Error,
                    messages.ToArray()
                );
            }
        }

        /// <summary>
        /// Post the specified text as markdown.
        /// </summary>
        /// <param name="chat">Extends the chat API client.</param>
        /// <param name="channelIdOrName">The name or ID of the channel.</param>
        /// <param name="text">The text (may include markdown)..</param>
        /// <returns>The ID of the created message.</returns>
        public static async Task<Timestamp> PostMarkdownMessage(this IChatApi chat, string channelIdOrName, string text)
        {
            if (chat == null) throw new ArgumentNullException(nameof(chat));

            var request = new PostMessageRequest
            {
                Channel = channelIdOrName,
                AsUser = true,
                UseMarkdown = true,
                Text = text,
            };

            var result = await chat.Post(request);
            result.EnsureSuccess();
            return result.Timestamp;
        }


        public static ElementValue? GetValue(this ViewState state, string id)
        {
            if (state.Values == null || state.Values.Count == 0) return null;

            var values = state.Values.SelectMany(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            if (values.ContainsKey(id))
            {
                return values[id];
            }

            return null;
        }

        public static async Task<ViewResponse> MakeJsonCall(this SlackWebApiClient client, string action, object data)
        {
            var c = client as IWebApiClient;
            var result = await c.MakeJsonCall<object, ViewResponse>(action, data);

            result.EnsureSuccess();

            return result;
        }
    }
}
