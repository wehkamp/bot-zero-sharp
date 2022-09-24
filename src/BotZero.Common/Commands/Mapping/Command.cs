using BotZero.Common.Commands.Mapping.Parameters;
using Mapster;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("BotZero.Common.UnitTests")]
namespace BotZero.Common.Commands.Mapping;

/// <summary>
/// A command stores a bunch of actions. It helps to prevent actions from
/// colliding with each other.
/// </summary>
internal class Command
{
    private readonly Regex _regex;
    private readonly List<CommandAction> _commands = new();
    private readonly string _invalidActionText;

    /// <summary>
    /// Creates a new command.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    internal Command(string name, string invalidActionText = "Sorry, I don't understand.")
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        _invalidActionText = invalidActionText ?? throw new ArgumentNullException(nameof(invalidActionText));
        _regex = new Regex($"^{Regex.Escape(name)}( |$)", RegexOptions.IgnoreCase);
    }

    internal Command AddAction(CommandAction command)
    {
        _commands.Add(command);
        return this;
    }

    internal Command AddAction(string name, Func<ActionContext, Task> action, params string[] alias)
    {
        _commands.Add(new CommandAction(name, action, alias, null));
        return this;
    }

    internal Command AddAction(string name, Func<ActionContext, Task> action, string[] alias, IParameter[]? parameters)
    {
        _commands.Add(new CommandAction(name, action, alias, parameters));
        return this;
    }

    internal Command AddAction(string name, Func<ActionContext, Task> action, params IParameter[]? parameters)
    {
        _commands.Add(new CommandAction(name, action, Array.Empty<string>(), parameters));
        return this;
    }

    internal string[] GetActionNames()
    {
        return _commands.SelectMany(x => x.GetActionNames()).ToArray();
    }

    internal async Task<bool> Process(ActionContext context)
    {
        if (!_regex.IsMatch(context.Message))
            return false;

        // strip tool from message
        context = context.Adapt<ActionContext>();
        context.Message = _regex.Replace(context.Message, "");
        context.Values = new Dictionary<string, object?>();
        context.Command = this;

        foreach (var command in _commands)
        {
            if (await command.Process(context))
            {
                return true;
            }
        }

        if (context.CommandMapper != null)
        {
            await context.CommandMapper.Reply(context, _invalidActionText);
        }

        return false;
    }
}
