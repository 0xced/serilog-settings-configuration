using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Settings.Configuration;

namespace WebSample;

/// <summary>
/// Controller to retrieve Serilog log level and filter switches and dynamically update them.
/// </summary>
/// <param name="accessor">The <see cref="ILogSwitchesAccessor"/> for accessing log level and filter switches.</param>
[ApiController]
[Route("logs")]
public class LogsController(ILogSwitchesAccessor accessor) : ControllerBase
{
    readonly IReadOnlyDictionary<string, LoggingLevelSwitch> _logLevelSwitches = accessor.LogLevelSwitches;
    readonly IReadOnlyDictionary<string, ILoggingFilterSwitch> _logFilterSwitches = accessor.LogFilterSwitches;

    /// <summary>
    /// Get all configured log level switches.
    /// </summary>
    /// <returns>A dictionary whose key is the switch name and the value is the log event level.</returns>
    [HttpGet("levels")]
    public IDictionary<string, LogEventLevel> GetLogLevelSwitches()
        => _logLevelSwitches.ToDictionary(e => e.Key, e => e.Value.MinimumLevel);

    /// <summary>
    /// Get all configured log filter switches.
    /// </summary>
    /// <returns>A dictionary whose key is the switch name and the value is the filter expression.</returns>
    [HttpGet("filters")]
    public IDictionary<string, string?> GetLogFilterSwitches()
        => _logFilterSwitches.ToDictionary(e => e.Key, e => e.Value.Expression);

    /// <summary>
    /// Update the minimum log level for a switch.
    /// </summary>
    /// <param name="name">The name of the log level switch.</param>
    /// <param name="level">The new minimum level to assign to the switch.</param>
    [HttpPost("levels")]
    public void SetLogLevelSwitches([LogLevelSwitchName] string name, [Required] LogEventLevel level)
        => _logLevelSwitches[name].MinimumLevel = level;

    /// <summary>
    /// Update the filter expression for a switch.
    /// </summary>
    /// <param name="name">The name of the log filter switch.</param>
    /// <param name="expression">
    /// The new filter expression to assign to the switch. See the <see href="https://github.com/serilog/serilog-expressions">Serilog Expressions</see> documentation for the syntax.
    /// </param>
    [HttpPost("filters")]
    public void SetLogFilterSwitches([LogFilterSwitchName] string name, [SerilogExpression] string expression)
        => _logFilterSwitches[name].Expression = expression;

    /// <summary>
    /// Log a verbose, debug, information, warning, error and fatal message.
    /// </summary>
    [HttpGet("test")]
    public void Test()
    {
        var logger = Log.ForContext<LogsController>();
        logger.Verbose("📖 This is a verbose log");
        logger.Debug("🐛 This is a debug log");
        logger.Information("ℹ️ This is an information log");
        logger.Warning("⚠️ This is a warning log");
        logger.Error("❌ This is an error log");
        logger.Fatal("💥 This is a fatal log");
    }
}
