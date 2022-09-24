using BotZero.Common.Commands;
using BotZero.Common.Slack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slack.NetStandard;
using Slack.NetStandard.Objects;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.Socket;
using Slack.NetStandard.WebApi;
using Slack.NetStandard.WebApi.Chat;
using Slack.NetStandard.WebApi.View;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BotZero.Common;

/// <summary>
/// Extension methods.
/// </summary>
public static class Extensions
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

            if(response.Errors != null)
            {
                messages.AddRange(response.Errors.Select(e => e.Message));
            }

            if(response is ViewResponse vr && vr.ResponseMetadata != null)
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

    /// <summary>
    /// Scans the calling assembly to search ICommand implementations and registers them into the DI collection.
    /// Will also register the HelpCommand and the CommandHandler.
    /// </summary>
    public static IServiceCollection AddCommands(this IServiceCollection provider)
    {
        return provider.AddCommands(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Scans the assemblies to search ICommand implementations and registers them into the DI collection.
    /// Will also register the HelpCommand and the CommandHandler.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="assemblies">The assemblies.</param>
    /// <returns>The DI collection for chaining.</returns>
    public static IServiceCollection AddCommands(this IServiceCollection provider, params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(ICommand)))
            .ToList();

        foreach (var type in types)
        {
            provider.AddTransient(type);
        }

        provider.AddTransient(services =>
        {
            var commands = new List<ICommand>();
            foreach (var type in types)
            {
                commands.Add((ICommand)services.GetRequiredService(type));
            }

            var profileService = services.GetRequiredService<SlackProfileService>();

            return new CommandHandler(
                services.GetRequiredService<SlackWebApiClient>(),
                profileService,
                services.GetRequiredService<ILogger<CommandHandler>>(),
                commands.ToArray());
        });

        provider.AddTransient(services =>
        {
            var handler = services.GetRequiredService<CommandHandler>();
            return handler.HelpCommand;
        });


        return provider;
    }

    /// <summary>
    /// Scans the calling assembly to search ISimpleSlackRequestHandler implementations and registers them into the DI collection.
    /// Will also register the HelpCommand and the CommandHandler.
    /// </summary>
    public static IServiceCollection AddSlackRequestHandlers(this IServiceCollection provider)
    {
        return provider.AddSlackRequestHandlers(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Scans the assemblies to search ISimpleSlackRequestHandler implementations and registers them into the DI collection.
    /// Will also register the HelpCommand and the CommandHandler.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <param name="assemblies">The assemblies.</param>
    /// <returns>The DI collection for chaining.</returns>
    public static IServiceCollection AddSlackRequestHandlers(this IServiceCollection provider, params Assembly[] assemblies)
    {
        var types = assemblies
            .SelectMany(x => x.GetTypes())
            .Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(ISlackRequestHandler<object?>)))
            .ToList();

        foreach (var type in types)
        {
            provider.AddTransient(type);
        }

        provider.AddTransient(services =>
        {
            var handlers = new List<ISlackRequestHandler<object?>>();
            foreach (var type in types)
            {
                handlers.Add((ISlackRequestHandler<object?>)services.GetRequiredService(type));
            }

            var commandHandler = services.GetService<CommandHandler>();
            if (commandHandler != null)
            {
                handlers.Add(commandHandler);
            }

            return new SlackRequestHandlerLocator(handlers.ToArray());
        });

        return provider;
    }

    /// <summary>
    /// Adds the Chat Bot to dependecy injection. The calling assembly is scanned
    /// for ICommand and IRequestHandler implementations.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns>The DI collection for chaining.</returns>
    public static IServiceCollection AddChatBot(this IServiceCollection provider)
    {
        return provider.AddChatBot(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Adds the Chat Bot to dependecy injection. The assemblies scanned
    /// for ICommand and IRequestHandler implementations.
    /// </summary>
    /// <param name="provider">The provider.</param>
    /// <returns>The DI collection for chaining.</returns>
    public static IServiceCollection AddChatBot(this IServiceCollection provider, params Assembly[] assemblies)
    {
        return provider
            .AddTransient(provider =>
            {
                var config = provider.GetRequiredService<IOptions<SlackConfiguration>>()?.Value;
                return new SlackWebApiClient(config?.Token);
            })
            .AddTransient<SlackProfileService>()
            .AddCommands(assemblies)
            .AddSlackRequestHandlers(assemblies)
            .AddSingleton(System.Threading.Channels.Channel.CreateUnbounded<Envelope>())
            .AddHostedService<SocketModeService>()
            .AddHostedService<ChatBot>();
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

    public static T? GetCustomAttributeImplementation<T>(this ParameterInfo info) where T : class
    {
        return info
            .GetCustomAttributes(true)
            .FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(T))) as T;
    }

    public static async Task<ViewResponse> MakeJsonCall(this SlackWebApiClient client, string action, object data)
    {
        var c = client as IWebApiClient;
        var result = await c.MakeJsonCall<object, ViewResponse>(action, data);

        result.EnsureSuccess();

        return result;
    }
}

