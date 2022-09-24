using BotZero.Common.Commands.Mapping.Attributes;

namespace BotZero.Common.Commands.Mapping.Parameters;

[AttributeUsage(AttributeTargets.Parameter)]
public class RegexParameterAttribute : ParameterAttributeBase
{
    private readonly string _regex;

    public RegexParameterAttribute(string regex)
    {
        _regex = regex;
    }

    public override IParameter GetParameter(string name)
    {
        return new RegexParameter(name, _regex);
    }
}
