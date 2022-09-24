using BotZero.Common.Slack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Slack.NetStandard.AsyncEnumerable;
using Slack.NetStandard.Socket;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace BotZero.Common;

public class SocketModeService : BackgroundService
{
    private SocketModeClient? _client;
    private readonly SlackConfiguration _config;
    private readonly Channel<Envelope> _channel;

    public SocketModeService(IOptions<SlackConfiguration> config, Channel<Envelope> channel)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _client = new SocketModeClient();
        await _client.ConnectAsync(_config.AppToken, cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_client == null) throw new Exception("Slack client was not initialized.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await foreach (var envelope in _client.EnvelopeAsyncEnumerable(stoppingToken))
            {
                await PassEnvelope(envelope, stoppingToken);
                Ack(envelope, stoppingToken);
            }
        }
    }

    private async Task PassEnvelope(Envelope envelope, CancellationToken token)
    {
        await _channel.Writer.WriteAsync(envelope, token);
    }

    private void Ack(Envelope envelope, CancellationToken stoppingToken)
    {
        // TODO: ack right away or wait a bit?
        // when we ack, the spinner goes away
        var task = new Task(async () =>
        {
            await Task.Delay(200);
            var ack = new Acknowledge { EnvelopeId = envelope.EnvelopeId };

            if (_client != null && _client.WebSocket != null && _client.WebSocket.State == WebSocketState.Open)
            {
                await _client.Send(JsonConvert.SerializeObject(ack), stoppingToken);
            }
        });

        task.Start();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client != null && _client.WebSocket != null && _client.WebSocket.State == WebSocketState.Open)
        {
            await _client.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "App shutting down", cancellationToken);
        }
    }
}
