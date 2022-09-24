using BotZero.Common.Commands.Mapping.Attributes;

namespace BotZero.Common.Commands.Mapping.Parameters;

[AttributeUsage(AttributeTargets.Parameter)]
public class ChoiceParameterAttribute : ParameterAttributeBase
{
    private readonly string[] _choices;
    private readonly string? _defaultValue;


    public ChoiceParameterAttribute(params string[] choices) : this(choices, null)
    {
    }

    public ChoiceParameterAttribute(string[] choices, string? defaultValue)
    {
        _choices = choices;
        _defaultValue = defaultValue;
    }

    public override IParameter GetParameter(string name)
    {
        return new ChoiceParameter(name, _choices, _defaultValue);
    }
}
