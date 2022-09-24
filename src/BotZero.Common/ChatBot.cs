using BotZero.Common.Slack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slack.NetStandard.Interaction;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.Socket;
using System.Threading.Channels;

namespace BotZero.Common;

/// <summary>
/// This background service acts as the chat bot, responding input.
/// The commands will respond to typed text.
/// </summary>
public class ChatBot : BackgroundService
{
    private readonly Channel<Envelope> _channel;
    private readonly ILogger<ChatBot> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SlackConfiguration _config;

    public ChatBot(
        Channel<Envelope> channel,
        ILogger<ChatBot> logger,
        IOptions<SlackConfiguration> config,
        IServiceScopeFactory scopeFactory)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var semaphore = new SemaphoreSlim(_config.MaxConcurrency);

        while (!stoppingToken.IsCancellationRequested)
        {
            await _channel.Reader.WaitToReadAsync(stoppingToken);

            if (_channel.Reader.TryRead(out var envelope))
            {
                if (envelope != null)
                {
                    Process(semaphore, envelope);
                }
            }
        }
    }

    private void Process(SemaphoreSlim semaphore, Envelope envelope)
    {
        var task = new Task(async () =>
        {
            semaphore.Wait();
            try
            {
                // Use a new scope for executing the handlers. Some might
                // need a scoped instance (like when it depends on EF)
                using var scope = _scopeFactory.CreateScope();

                // set the right user name and email
                if(envelope.Payload is BlockActionsPayload bap)
                {
                    var profileService = scope.ServiceProvider.GetRequiredService<SlackProfileService>();
                    var user = await profileService.GetUser(bap.User.ID);
                    if (user != null)
                    {
                        bap.User.Name = user.Name;
                        bap.User.Email = user.Email;
                    }
                }

                var locator = scope.ServiceProvider.GetRequiredService<SlackRequestHandlerLocator>();
                var pipeline = new SlackPipeline<object?>(locator.Handlers);
                await pipeline.Process(envelope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing the pipeline.");
                //an error here should not break/kill the application
                //throw;
            }
            finally
            {
                semaphore.Release();
            }
        });

        task.Start();
    }
}