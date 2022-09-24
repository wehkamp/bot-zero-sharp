using BotZero.Common.Commands.Mapping.Attributes;

namespace BotZero.Common.Commands.Mapping.Parameters;

public class IntParameterAttribute : ParameterAttributeBase
{
    private readonly int? _defaultValue = null;

    public IntParameterAttribute()
    {

    }

    public IntParameterAttribute(int defaultValue)
    {
        _defaultValue = defaultValue;
    }

    public override IParameter GetParameter(string name)
    {
        return new IntParameter(name, _defaultValue);
    }
}
