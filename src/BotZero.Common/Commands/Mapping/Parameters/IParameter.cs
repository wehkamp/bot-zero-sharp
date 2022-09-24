namespace BotZero.Common.Commands.Mapping.Parameters;

public interface IParameter
{
    string Name { get; }

    string GetRegex();

    object? ConvertValue(string value);

    bool IsOptional { get; }
}
