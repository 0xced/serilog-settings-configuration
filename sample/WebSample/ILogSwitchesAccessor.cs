using Serilog.Core;
using Serilog.Settings.Configuration;

namespace WebSample;

public interface ILogSwitchesAccessor
{
    IReadOnlyDictionary<string, LoggingLevelSwitch> LogLevelSwitches { get; }
    IReadOnlyDictionary<string, ILoggingFilterSwitch> LogFilterSwitches { get; }
}
