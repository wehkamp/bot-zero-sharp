using BotZero.Common.Commands.Mapping.Parameters;

namespace BotZero.Common.Commands.Mapping.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public abstract class ParameterAttributeBase : Attribute, IParameterAttribute
{
    public abstract IParameter GetParameter(string name);
}

