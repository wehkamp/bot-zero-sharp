namespace BotZero.Common.Commands.Mapping.Parameters;

public abstract class ParameterBase : IParameter
{
    public string Name { get; }

    public abstract bool IsOptional { get; }

    public ParameterBase(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public abstract string GetRegex();

    public virtual object? ConvertValue(string value)
    {
        return value;
    }
}
