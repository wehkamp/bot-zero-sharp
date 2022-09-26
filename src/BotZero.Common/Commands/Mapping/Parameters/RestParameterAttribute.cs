using BotZero.Common.Commands.Mapping.Attributes;

namespace BotZero.Common.Commands.Mapping.Parameters;

[AttributeUsage(AttributeTargets.Parameter)]
public class RestParameterAttribute : ParameterAttributeBase
{
    public RestParameterAttribute(bool isOptional = false) => IsOptional = isOptional;

    protected bool IsOptional { get; }

    public override IParameter GetParameter(string name)
    {
        return new RestParameter(name, IsOptional);
    }
}
