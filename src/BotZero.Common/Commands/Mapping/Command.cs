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

    internal string[] GetActionNames()
    {
        return _commands.SelectMany(x => x.GetActionNames()).ToArray();
    }

    internal async Task<bool> Process(ActionContext context)
    {
        if (!_regex.IsMatch(context.Message))
            return false;

        var commandMapper = context.CommandMapper;

        context = context.Adapt<ActionContext>();
        context.Values = new Dictionary<string, object?>();
        context.Command = this;

        // strip command name (or alias) from message
        context.Message = _regex.Replace(context.Message, "");

        try
        {
            foreach (var command in _commands)
            {
                if (await command.Process(context))
                {
                    return true;
                }
            }
        }
        catch (NotAuthorizedException naex)
        {
            if (commandMapper != null)
            {
                var msg = $"Sorry, you are not authorized to execute `{naex.CommandName.ToLower()}.{naex.ActionName.ToLower()}`.";
                await commandMapper!.Reply(context, msg);
            }

            return true;
        }

        if (commandMapper != null)
        {
            await commandMapper.Reply(context, _invalidActionText);
        }

        return false;
    }
}
