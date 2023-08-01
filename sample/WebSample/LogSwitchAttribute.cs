using System.ComponentModel.DataAnnotations;

namespace WebSample;

abstract class LogSwitchAttribute : RequiredAttribute
{
    protected abstract string SwitchTypeName { get; }

    protected abstract IEnumerable<string> GetAvailableNames(ILogSwitchesAccessor accessor);

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string name)
        {
            var names = GetAvailableNames(validationContext.GetRequiredService<ILogSwitchesAccessor>());
            if (names.Contains(name))
            {
                return ValidationResult.Success;
            }

            var error = $"There is no log {SwitchTypeName} switch named '{name}'. " +
                        $"The available names are: '{string.Join("', '", names.Order())}'";
            return new ValidationResult(error);
        }

        return base.IsValid(value, validationContext);
    }
}

class LogLevelSwitchNameAttribute : LogSwitchAttribute
{
    protected override string SwitchTypeName => "level";
    protected override IEnumerable<string> GetAvailableNames(ILogSwitchesAccessor accessor) => accessor.LogLevelSwitches.Keys;
}

class LogFilterSwitchNameAttribute : LogSwitchAttribute
{
    protected override string SwitchTypeName => "filter";
    protected override IEnumerable<string> GetAvailableNames(ILogSwitchesAccessor accessor) => accessor.LogFilterSwitches.Keys;
}
