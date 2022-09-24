using System.Text.RegularExpressions;

namespace BotZero.Common.Commands.Mapping.Parameters;

internal class RestParameter : ParameterBase
{
    public RestParameter(string name, bool isOptional = true) : base(name)
    {
        IsOptional = isOptional;
    }

    public override bool IsOptional { get; }

    public override string GetRegex()
    {
        var regex = $"( (?<{Regex.Escape(Name)}>.+))";
        if (IsOptional) regex += "?";
        return regex;
    }
}
