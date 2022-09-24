using BotZero.Common.Commands;
using BotZero.Common.Slack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slack.NetStandard;
using Slack.NetStandard.RequestHandler;
using Slack.NetStandard.Socket;
using System.Reflection;

namespace BotZero.Common;

/// <summary>
/// Extension methods.
/// </summary>
public static class Extensions
{
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
                // make sure the command handler is executed first,
                // this helps to display errors in the UI
                handlers.Insert(0, commandHandler);
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

   

   

}

