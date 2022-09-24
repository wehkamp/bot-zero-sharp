using System.Text.RegularExpressions;

namespace BotZero.Common.Commands.Mapping.Parameters;

internal class StringParameter : ParameterBase
{
    private readonly string? _defaultValue;

    public override bool IsOptional => _defaultValue != null;

    /// <summary>
    /// Creates a new string parameter.
    /// </summary>
    /// <param name="namsxze">The name of the parameter.</param>
    /// <param name="defaultValue">The default value or <c>null</c> if the parameter is not optional.</param>
    public StringParameter(string name, string? defaultValue = null) : base(name)
    {
        _defaultValue = defaultValue;
    }

    public override string GetRegex()
    {
        var str = $"( (?<{Regex.Escape(Name)}>(\"[^\"]+\")|([^ ]+)))";
        if (_defaultValue != null)
        {
            str += "?";
        }

        return str;
    }

    public override object? ConvertValue(string value)
    {
        if (string.IsNullOrEmpty(value) && _defaultValue != null)
        {
            return _defaultValue;
        }

        if (value.StartsWith('"') && value.EndsWith('"'))
        {
            return value[1..^1];
        }

        return base.ConvertValue(value);
    }
}
