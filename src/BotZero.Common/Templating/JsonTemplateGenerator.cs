namespace BotZero.Common.Templating;

using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Newtonsoft.Json;

public class JsonTemplateGenerator : IJsonTemplateGenerator
{
    public IHandlebars Handlebars { get; }

    public JsonTemplateGenerator()
    {
        Handlebars = HandlebarsDotNet.Handlebars.Create();
        Handlebars.Configuration.TextEncoder = new JsonTextEncoder();
        Handlebars.Configuration.ObjectDescriptorProviders.Add(new FlaggedEnumObjectDescriptorProvider());

        Handlebars.RegisterHelper("ifEq", (output, options, context, arguments) =>
        {
            if (arguments[0].ToString() == arguments[1].ToString())
            {
                options.Template(output, context);
            }
            else
            {
                options.Inverse(output, context);
            }
        });

        HandlebarsHelpers.Register(Handlebars);
    }

    public HandlebarsTemplate<object, object> Compile(string template) => Handlebars.Compile(template);

    public string Parse(string template, object input)
    {
        var t = Compile(template);
        var json = t(input);
        Deserialize(json);
        return json;
    }

    public dynamic? ParseToObject(string template, object input)
    {
        var t = Compile(template);
        var json = t(input);
        return Deserialize(json);
    }

    private static dynamic? Deserialize(string json)
    {
        try
        {
            // replace tabs with spaces, as tabs make the JSON in the
            // console unreadable:
            json = json.Replace("\t", "  ");

            return JsonConvert.DeserializeObject<dynamic>(json);
        }
        catch (Exception ex)
        {
            throw new InvalidJsonException(ex, json);
        }
    }
}

