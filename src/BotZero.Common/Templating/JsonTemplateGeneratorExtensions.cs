using System.Reflection;

namespace BotZero.Common.Templating;

public static class JsonTemplateGeneratorExtensions
{
    /// <summary>
    /// Parses the input data with a JSON template embedded in the calling assembly.
    /// </summary>
    /// <param name="generator">The JSON generator.</param>
    /// <param name="name">The full name of the manifest resouce.</param>
    /// <param name="input">The input data that is fed to the template.</param>
    /// <returns>The JSON result.</returns>
    public static string ParseWithManifestResource(this IJsonTemplateGenerator generator, string name, object input)
    {
        return generator.ParseWithManifestResource(Assembly.GetCallingAssembly(), name, input);
    }

    /// <summary>
    /// Parses the input data with a JSON template embedded in the calling assembly to a dynamic object.
    /// </summary>
    /// <param name="generator">The JSON generator.</param>
    /// <param name="name">The full name of the manifest resouce.</param>
    /// <param name="input">The input data that is fed to the template.</param>
    /// <returns>A dynamic object.</returns>
    public static dynamic? ParseWithManifestResourceToObject(this IJsonTemplateGenerator generator, string name, object input)
    {
        return generator.ParseWithManifestResourceToObject(Assembly.GetCallingAssembly(), name, input);
    }

    /// <summary>
    /// Parses the input data with a JSON template embedded in the assembly.
    /// </summary>
    /// <param name="generator">The JSON generator.</param>
    /// <param name="assembly">The assembly.</param>
    /// <param name="name">The full name of the manifest resouce.</param>
    /// <param name="input">The input data that is fed to the template.</param>
    /// <returns>The JSON result.</returns>
    public static string ParseWithManifestResource(this IJsonTemplateGenerator generator, Assembly assembly, string name, object input)
    {
        var template = GetManifestTemplate(assembly, name, input);
        return generator.Parse(template, input);
    }

    /// <summary>
    /// Parses the input data with a JSON template embedded in the assembly into a dynamic object.
    /// </summary>
    /// <param name="generator">The JSON generator.</param>
    /// <param name="assembly">The assembly.</param>
    /// <param name="name">The full name of the manifest resouce.</param>
    /// <param name="input">The input data that is fed to the template.</param>
    /// <returns>A dynamic object.</returns>
    public static dynamic? ParseWithManifestResourceToObject(this IJsonTemplateGenerator generator, Assembly assembly, string name, object input)
    {
        var template = GetManifestTemplate(assembly, name, input);
        return generator.ParseToObject(template, input);
    }

    private static string GetManifestTemplate(Assembly assembly, string name, object input)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        using var stream = assembly.GetManifestResourceStream(name);
        if (stream == null) throw new Exception($"Manifest resouce with name '{name}' in assembly '{assembly.FullName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}