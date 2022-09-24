namespace BotZero.Common.Commands.Mapping.Attributes;

/// <summary>
/// Incidates the method can be used to execute an action. The name of the
/// action is derived from the method. The name is not case sensitive.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ActionAttribute : Attribute
{
    /// <summary>
    /// Creates a new action attribute.
    /// </summary>
    /// <param name="alias">Different names to which your action reacts.</param>
    public ActionAttribute(params string[] alias)
    {
        Alias = alias;
    }

    public string[] Alias { get; }
}
