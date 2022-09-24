using BotZero.Common.Commands.Mapping.Attributes;
using BotZero.Common.Commands.Mapping.Parameters;
using BotZero.Common.Messaging;
using BotZero.Slack.Common.Commands;
using Mapster;
using Slack.NetStandard;
using System.Reflection;

namespace BotZero.Common.Commands.Mapping;

/// <summary>
/// Command mapper that uses attributes to automatically map commands and parameters into a tool.
/// </summary>
public abstract class CommandMapper : CommandBase
{
    private const BindingFlags _commandMethodBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Public;
    private readonly Lazy<Command> _command;

    protected CommandMapper(SlackWebApiClient client) : base(client)
    {
        _command = new Lazy<Command>(() => Create());
    }

    public override async Task<bool> Process(CommandContext context)
    {
        var c = context.Adapt<ActionContext>();
        c.CommandMapper = this;
        var command = _command.Value;
        return await command.Process(c);
    }

    /// <summary>
    /// The current context that is executing the command.
    /// </summary>
    public ActionContext Context { get; protected set; } = new ActionContext();

    internal Command Create()
    {
        // determine name of the tool
        var name = GetType().GetCustomAttribute<CommandAttribute>(true)?.Name;
        if (string.IsNullOrEmpty(name))
        {
            name = GetType().Name.Replace("Command", "").ToLower();
        }

        var hasHelp = GetHelpLines().FirstOrDefault() != null;
        var invalidActionText = hasHelp ? $"Sorry, I don't understand. Ask `help {name}` for more information." : "Sorry, I don't understand.";
        var command = new Command(name, invalidActionText);
        var actions = GetActionMethods();

        // parse methods into commands
        foreach (var (method, attribute) in actions)
        {
            name = method.Name;
            var parameters = new List<IParameter>();

            foreach (var p in method.GetParameters())
            {
                if (p.Name == null)
                {
                    throw new NotSupportedException($"Parameter without name is not supported.");
                }

                var pa = GetCustomAttributeImplementation<IParameterAttribute>(p);
                if (pa != null)
                {
                    var parameter = pa.GetParameter(p.Name);
                    parameters.Add(parameter);
                }
                else if (p.ParameterType == typeof(int))
                {
                    parameters.Add(new IntParameter(p.Name, p.DefaultValue as int?));
                }
                else if (p.ParameterType == typeof(string))
                {
                    parameters.Add(new StringParameter(p.Name, p.DefaultValue as string));
                }
                else
                {
                    throw new NotSupportedException($"Parameter '{p.Name}' with type '{p.ParameterType.FullName}' is not supported. Hint: a string always works.");
                }
            }

            command.AddAction(new CommandAction(
                name,
                cm => Invoke(method, cm, name),
                attribute.Alias,
                parameters.ToArray()
            ));
        }

        return command;
    }

    /// <summary>
    /// Invokes the specified method.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="context">The context.</param>
    /// <param name="actionName">The name of the action.</param>
    private async Task Invoke(MethodInfo method, ActionContext context, string actionName)
    {
        var values = context.Values.Select(x => x.Value).ToList();

        Context = context.Adapt<ActionContext>();
        Context.CommandMapper = this;
        Context.ActionName = actionName;

        var firstParameter = method.GetParameters().FirstOrDefault();
        if (firstParameter?.ParameterType.IsAssignableFrom(typeof(ActionContext)) == true)
        {
            values.Insert(0, context);
        }

        if (method.Invoke(this, values.ToArray()) is Task task)
        {
            await task;
        }
    }

    /// <summary>
    /// Echos a message from the bot to the channel.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="msg">The message.</param>
    /// <returns>The ID of the message.</returns>
    protected Task<Timestamp> Send(string msg)
    {
        return Send(Context, msg);
    }

    /// <summary>
    /// Echos a reply to a user. If the context is not a direct message, the user will be mentioned.
    /// </summary>
    /// <param name="msg">The messasge.</param>
    /// <returns>The ID of the message.</returns>
    protected Task Reply(string msg)
    {
        return Reply(Context, msg);
    }

    /// <summary>
    /// Creates an updatable message which can be updated multiple times.
    /// </summary>
    /// <returns>The message.</returns>
    public UpdatableMessage CreateUpdatableMessage()
    {
        return CreateUpdatableMessage(Context);
    }

    /// <summary>
    /// Gets the action methods. These methods are decorated with the <c>ActionAttribute</c>.
    /// </summary>
    /// <returns>Tuples with each command.</returns>
    private IEnumerable<(MethodInfo method, ActionAttribute attribute)> GetActionMethods()
    {
        bool yieldDefault = true;
        var methods = GetType().GetMethods(_commandMethodBindingFlags);

        foreach (var method in methods)
        {
            var action = method.GetCustomAttribute<ActionAttribute>(true);
            if (action != null)
            {
                yield return (method, action);
            }
        }

        if (yieldDefault)
        {

            // if no action methods are found, search for the default
            // Callback method and return it with an empty alias:
            var callback = GetType().GetMethod("Callback", _commandMethodBindingFlags);
            if (callback != null)
            {
                yield return (callback, new ActionAttribute(""));
            }
        }
    }

    public static T? GetCustomAttributeImplementation<T>(ParameterInfo info) where T : class
    {
        return info
            .GetCustomAttributes(true)
            .FirstOrDefault(x => x.GetType().IsAssignableTo(typeof(T))) as T;
    }
}
