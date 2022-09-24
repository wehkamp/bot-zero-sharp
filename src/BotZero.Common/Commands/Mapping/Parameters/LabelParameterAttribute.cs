using BotZero.Common.Commands.Mapping.Attributes;

namespace BotZero.Common.Commands.Mapping.Parameters;

[AttributeUsage(AttributeTargets.Parameter)]
public class LabelParameterAttribute : ParameterAttributeBase
{
    private readonly string _label;
    private readonly bool _isOptional;

    public LabelParameterAttribute(string label, bool isOptional = false)
    {
        _label = label;
        _isOptional = isOptional;
    }

    public override IParameter GetParameter(string name)
    {
        return new LabelParameter(name, _label, _isOptional);
    }
}
