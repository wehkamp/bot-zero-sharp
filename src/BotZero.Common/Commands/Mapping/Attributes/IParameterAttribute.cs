using BotZero.Common.Commands.Mapping.Parameters;

namespace BotZero.Common.Commands.Mapping.Attributes;

public interface IParameterAttribute
{
    IParameter GetParameter(string name);
}

