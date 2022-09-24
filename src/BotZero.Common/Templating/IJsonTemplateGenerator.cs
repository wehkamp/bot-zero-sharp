using HandlebarsDotNet;

namespace BotZero.Common.Templating;

public interface IJsonTemplateGenerator
{
    IHandlebars Handlebars { get; }

    HandlebarsTemplate<object, object> Compile(string template);

    string Parse(string template, object input);

    dynamic? ParseToObject(string template, object input);
}
