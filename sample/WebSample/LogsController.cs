using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Settings.Configuration;

namespace WebSample;

[ApiController]
[Route("logs")]
public class LogsController : ControllerBase
{
    readonly IReadOnlyDictionary<string, LoggingLevelSwitch> _logLevelSwitches;
    readonly IReadOnlyDictionary<string, ILoggingFilterSwitch> _logFilterSwitches;

    public LogsController(ILogSwitchesAccessor accessor)
    {
        _logLevelSwitches = accessor.LogLevelSwitches;
        _logFilterSwitches = accessor.LogFilterSwitches;
    }

    [HttpGet("levels")]
    public IDictionary<string, LogEventLevel> GetLogLevelSwitches()
        => _logLevelSwitches.ToDictionary(e => e.Key, e => e.Value.MinimumLevel);

    [HttpGet("filters")]
    public IDictionary<string, string?> GetLogFilterSwitches()
        => _logFilterSwitches.ToDictionary(e => e.Key, e => e.Value.Expression);

    [HttpPost("levels")]
    public void SetLogLevelSwitches([LogLevelSwitchName] string name, [Required] LogEventLevel level)
        => _logLevelSwitches[name].MinimumLevel = level;

    [HttpPost("filters")]
    public void SetLogFilterSwitches([LogFilterSwitchName] string name, [SerilogExpression] string expression)
        => _logFilterSwitches[name].Expression = expression;

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
