using System.Text.RegularExpressions;

namespace BotZero.Common.Commands.Mapping.Parameters;

internal class RegexParameter : ParameterBase
{
    private readonly string _regex;

    public RegexParameter(string name, string regex) : base(name)
    {
        _regex = regex;
    }

    public override bool IsOptional => false;

    public override string GetRegex()
    {
        return $"( (?<{Regex.Escape(Name)}>({_regex})))";
    }
}
