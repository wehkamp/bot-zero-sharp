using System.Text.RegularExpressions;

namespace BotZero.Common.Commands.Mapping.Parameters;

internal class ChoiceParameter : ParameterBase
{
    private readonly string[] _choices;
    private readonly string? _defaultValue;

    public ChoiceParameter(string name, string[] choices, string? defaultValue = null) : base(name)
    {
        _choices = choices;
        _defaultValue = defaultValue;
    }

    public override bool IsOptional => _defaultValue != null;

    public override string GetRegex()
    {
        var choices = string.Join("|", _choices.Select(x => Regex.Escape(x)));

        var str = $"( (?<{Regex.Escape(Name)}>({choices})))";
        if (IsOptional)
        {
            str += "?";
        }

        return str;
    }

    public override object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value) && _defaultValue != null) return _defaultValue;

        return base.ConvertValue(value);
    }
}
