using System.Text.RegularExpressions;

namespace BotZero.Common.Commands.Mapping.Parameters;

internal class LabelParameter : ParameterBase
{
    private readonly string _label;
    private readonly bool _isOptional;

    public LabelParameter(string name, string label, bool isOptional = false) : base(name)
    {
        _label = label;
        _isOptional = isOptional;
    }

    public override bool IsOptional => _isOptional;

    public override string GetRegex()
    {
        var str = $"( (?<{Regex.Escape(Name)}>({Regex.Escape(_label)})))";
        if (IsOptional)
        {
            str += "?";
        }

        return str;
    }

    public override object ConvertValue(string value)
    {
        return _label;
    }
}
