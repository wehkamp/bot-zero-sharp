namespace BotZero.Common.Commands.Mapping.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class HelpAttribute : Attribute
{
    public HelpAttribute(params string[] help)
    {
        Help = help;
    }

    public string[] Help { get; }
}