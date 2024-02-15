using System.Collections.Frozen;
using Serilog.Core;
using Serilog.Settings.Configuration;

namespace WebSample;

class LogSwitchesAccessor : ILogSwitchesAccessor
{
    IReadOnlyDictionary<string, LoggingLevelSwitch>? _logLevelSwitches;
    IReadOnlyDictionary<string, ILoggingFilterSwitch>? _logFilterSwitches;

    public Dictionary<string, LoggingLevelSwitch> LogLevelSwitches { get; } = new();
    public Dictionary<string, ILoggingFilterSwitch> LogFilterSwitches { get; } = new();

    IReadOnlyDictionary<string, LoggingLevelSwitch> ILogSwitchesAccessor.LogLevelSwitches
        => _logLevelSwitches ??= LogLevelSwitches.ToFrozenDictionary();

    IReadOnlyDictionary<string, ILoggingFilterSwitch> ILogSwitchesAccessor.LogFilterSwitches
        => _logFilterSwitches ??= LogFilterSwitches.ToFrozenDictionary();
}
