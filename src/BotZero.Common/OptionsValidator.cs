using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace BotZero.Common
{
    public static class OptionsValidator
    {
        public static void ValidateByDataAnnotation(object instance, string sectionName)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(instance);
            var valid = Validator.TryValidateObject(instance, context, validationResults);
            if (valid) return;

            var msg = string.Join("\n", validationResults.Select(r => r.ErrorMessage));
            throw new ConfigurationErrorsException($"Invalid configuration of section '{sectionName}':\n{msg}");
        }

        public static IServiceCollection ConfigureAndValidate<TOptions>(
            this IServiceCollection services,
            string sectionName,
            IConfiguration configuration)
            where TOptions : class
        {
            var section = configuration.GetSection(sectionName);
            if (section == null) throw new ConfigurationErrorsException($"Invalid configuration of section '{sectionName}': not found.");

            return services
                .Configure<TOptions>(section)
                .PostConfigure<TOptions>(x => ValidateByDataAnnotation(x, sectionName));
        }
    }
}
