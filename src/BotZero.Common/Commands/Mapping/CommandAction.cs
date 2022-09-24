using BotZero.Common.Commands.Mapping.Parameters;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace BotZero.Common.Commands.Mapping;

/// <summary>
/// An action enables the bot to respond to user input. Actions may have parameters
/// that are mapped to values when the action is executed. All actions have a name by 
/// default. An alias can be registered to respond to alternatives. An empty alias <c>""</c>
/// can be used if the name of the action is optional. The rest of the actions are
/// inspected to make sure no collision with other actions happend.
/// Actions and parameters are not case sensitive.
/// </summary>
[DebuggerDisplay("CommandAction: {Name}")]
internal class CommandAction
{
    internal string Name { get; }

    private readonly Func<ActionContext, Task> _action;
    private readonly string[] _alias;
    private readonly IParameter[]? _parameters;
    private Regex? _commandRegex;
    private Regex? _parameterRegex;

    internal CommandAction(string name, Func<ActionContext, Task> action, string[]? alias, IParameter[]? parameters)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
        Name = name;
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _alias = alias ?? Array.Empty<string>();
        _parameters = parameters ?? Array.Empty<IParameter>();
    }

    /// <summary>
    /// Gets the action name and the aliases.
    /// Skips the default action name (<c>""</c>).
    /// </summary>
    /// <returns>Action names.</returns>
    internal string[] GetActionNames()
    {
        var commands = new List<string>
        {
            Name
        };

        commands.AddRange(_alias.Where(x => x != ""));
        return commands.Select(x => x.ToLower()).ToArray();
    }

    internal async Task<bool> Process(ActionContext context)
    {
        if (_commandRegex == null)
            _commandRegex = CreateActionRegex(context);

        var message = context.Message;
        var match = _commandRegex.Match(message);

        if (!match.Success) return false;

        // remove command from message
        message = _commandRegex.Replace(message, "");

        // if optional argument -- mimic it was there
        var isOptional = _alias.Contains("");
        if (
            isOptional &&
            _parameters != null &&
            _parameters.Length > 0 &&
            message != "" &&
            !message.StartsWith(" "))
        {
            message = " " + message;
        }

        // validate parameters
        if (_parameterRegex == null)
            _parameterRegex = CreateParameterRegex(context);

        match = _parameterRegex.Match(message);
        if (!match.Success) return false;

        if (_parameters != null)
        {
            foreach (var p in _parameters)
            {
                var grp = match.Groups[p.Name];
                var value = p.ConvertValue(grp.Value);

                context.Values.Add(p.Name, value);
            }
        }

        // execute action
        context.ActionName = Name;
        await _action(context);

        return true;
    }

    private Regex CreateParameterRegex(ActionContext context)
    {
        var bob = new StringBuilder();
        bob.Append('^');
        if (_parameters != null)
        {
            foreach (var p in _parameters)
            {
                bob.Append(p.GetRegex());
            }
        }
        bob.Append('$');
        return new Regex(bob.ToString(), RegexOptions.IgnoreCase);
    }

    private Regex CreateActionRegex(ActionContext context)
    {
        var isOptional = _alias.Contains("");
        var hasParameters = _parameters?.Length > 0;
        var bob = new StringBuilder();

        var myActionNames = GetActionNames();

        bob.Append("^(((");
        bob.Append(string.Join("|", myActionNames.Select(x => Regex.Escape(x))));
        bob.Append(')');

        if (hasParameters)
        {
            //don't match partials from other tools!
            bob.Append("(?=( |$))");
        }

        bob.Append(')');


        if (isOptional && hasParameters)
        {
            if (context.Command == null) throw new InvalidOperationException("Context is missing a command.");

            // filter out other commands:
            var otherCommands = context
                .Command
                .GetActionNames()
                .Distinct()
                .Where(x => !myActionNames.Contains(x))
                .Select(x => Regex.Escape(x))
                .ToArray();

            bob.Append("|(?!(");
            bob.Append(string.Join("|", otherCommands));

            //don't match partials from other tools!
            bob.Append("(?=( |$))");

            bob.Append("))");

            if (otherCommands.Length == 0)
            {
                bob.Append("|$");
            }
        }

        bob.Append(')');


        if (!hasParameters)
        {
            bob.Append("?$");
        }

        return new Regex(bob.ToString(), RegexOptions.IgnoreCase);
    }
}

