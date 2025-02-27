using Serilog.Core;
using Serilog.Settings.Configuration;

namespace WebSample;

/// <summary>
/// Access to dynamically created log level switches and log filter filters.
/// </summary>
public interface ILogSwitchesAccessor
{
    /// <summary>
    /// Log level switches created either from the <c>Serilog:LevelSwitches</c> section (declared switches) or the <c>Serilog:MinimumLevel:Override</c> section (minimum level override switches).
    /// </summary>
    IReadOnlyDictionary<string, LoggingLevelSwitch> LogLevelSwitches { get; }

    /// <summary>
    /// Log filter switches created from the <c>Serilog:FilterSwitches</c> section of the configuration.
    /// </summary>
    IReadOnlyDictionary<string, ILoggingFilterSwitch> LogFilterSwitches { get; }
}
