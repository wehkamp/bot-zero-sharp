using Mapster;
using MarkdownDeep;
using Slack.NetStandard;
using Slack.NetStandard.Messages.Blocks;
using Slack.NetStandard.WebApi.Chat;

namespace BotZero.Common.Messaging;
public class UpdatableMessage
{
    private readonly object _mutex = new();
    private readonly SlackWebApiClient _slackClient;

    /// <summary>
    /// Removes markdown from a text and returns the entire text.
    /// </summary>
    private static readonly Markdown _markdownRemover = new()
    {
        SummaryLength = -1
    };

    private Thread? _thread;
    private Message? _latestMessage;
    public Timestamp? Timestamp { get; set; }

    public string? Channel { get; set; }

    public UpdatableMessage(SlackWebApiClient slackClient, string? channel = null, Timestamp? timestamp = null)
    {
        _slackClient = slackClient ?? throw new ArgumentNullException(nameof(slackClient));
        Channel = channel;
        Timestamp = timestamp;
    }

    private void SendByThread()
    {
        lock (_mutex)
        {
            if (_thread != null)
            {
                return;
            }

            _thread = new Thread(async () =>
            {
                Message? message = null;

                while (true)
                {
                    lock (_mutex)
                    {
                        if (_latestMessage == null)
                        {
                            _thread = null;
                            return;
                        }

                        message = _latestMessage;
                        _latestMessage = null;
                    }

                    if (Timestamp == null)
                        await Post(message);
                    else
                        await Update(message);

                    // send max 4 messages per second to avoid
                    // Slack from rate limitting us.
                    if (_latestMessage != null)
                        await Task.Delay(250);
                }
            });

            _thread.Start();
        }
    }

    private async Task Update(Message message)
    {
        var item = message.Adapt<UpdateMessageRequest>();
        item.Channel = Channel;
        item.Timestamp = Timestamp;
        var response = await _slackClient.Chat.Update(item);
        response.EnsureSuccess();
    }

    private async Task Post(Message message)
    {
        var item = message.Adapt<PostMessageRequest>();
        item.Channel = Channel;
        var response = await _slackClient.Chat.Post(item);
        response.EnsureSuccess();

        // don't forget to save the timestamp and channel
        // for later
        lock (_mutex)
        {
            Timestamp = response.Timestamp;
            Channel = response.Channel;
        }
    }

    public void Send(Message message)
    {
        lock (_mutex)
        {
            _latestMessage = message;
            Timestamp ??= message.Timestamp;
            Channel ??= message.Channel;
        }

        if (Channel == null)
        {
            throw new Exception("No channel set.");
        }

        SendByThread();
    }

    public async Task SendAsync(Message message)
    {
        Send(message);
        await WaitForDelivery();
    }

    public void Send(string text)
    {
        var msg = new Message
        {
            Text = _markdownRemover.Transform(text),
        };

        msg.Blocks.Add(new Section
        {
            Text = new MarkdownText(text)
        });

        Send(msg);
    }

    public async Task SendAsync(string text)
    {
        Send(text);
        await WaitForDelivery();
    }


    public async Task WaitForDelivery()
    {
        while (true)
        {
            if (_thread != null)
            {
                await Task.Delay(10);
            }
            else if (_latestMessage != null)
            {
                SendByThread();
            }
            else
            {
                break;
            }
        }
    }

    public class Message
    {
        public string? Channel { get; set; }

        public Timestamp? Timestamp { get; set; }

        public string? Text { get; set; }

        public List<IMessageBlock> Blocks { get; set; } = new List<IMessageBlock>();
    }
}