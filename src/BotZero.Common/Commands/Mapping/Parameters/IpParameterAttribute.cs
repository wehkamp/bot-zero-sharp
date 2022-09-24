using BotZero.Common.Commands.Mapping.Attributes;

namespace BotZero.Common.Commands.Mapping.Parameters;

[AttributeUsage(AttributeTargets.Parameter)]
public class IpParameterAttribute : ParameterAttributeBase
{
    public override IParameter GetParameter(string name)
    {
        return new IpParameter(name);
    }
}
