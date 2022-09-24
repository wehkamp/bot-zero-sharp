using System.Text.RegularExpressions;

namespace BotZero.Common.Commands.Mapping.Parameters;

internal class IntParameter : ParameterBase
{
    private readonly int? _defaultValue;

    public override bool IsOptional => _defaultValue != null;


    public IntParameter(string name, int? defaultValue = null) : base(name)
    {
        _defaultValue = defaultValue;
    }

    public override string GetRegex()
    {
        var str = $"( (?<{Regex.Escape(Name)}>\\-?\\d+))";
        if (IsOptional) str += "?";
        return str;
    }

    public override object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value) && IsOptional)
            return _defaultValue;

        return Convert.ToInt32(value);
    }
}
