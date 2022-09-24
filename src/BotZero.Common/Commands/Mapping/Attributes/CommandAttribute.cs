namespace BotZero.Common.Commands.Mapping.Attributes;

/// <summary>
/// Can be used to override the name of the command.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute : Attribute
{
    public CommandAttribute(string name = "")
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }
}
