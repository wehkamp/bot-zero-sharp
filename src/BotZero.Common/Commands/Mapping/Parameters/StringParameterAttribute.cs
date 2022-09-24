using BotZero.Common.Commands.Mapping.Attributes;

namespace BotZero.Common.Commands.Mapping.Parameters;

public class StringParameterAttribute : ParameterAttributeBase
{
    private readonly string? _defaultValue;

    public StringParameterAttribute(string? defaultValue = null)
    {
        _defaultValue = defaultValue;
    }

    public override IParameter GetParameter(string name)
    {
        return new StringParameter(name, _defaultValue);
    }
}
