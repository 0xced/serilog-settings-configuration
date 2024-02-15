using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Expressions;
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
    {
        return _logLevelSwitches.ToDictionary(e => e.Key, e => e.Value.MinimumLevel);
    }

    [HttpPost("levels")]
    public IActionResult SetLogLevelSwitches([Required] string name, [Required] LogEventLevel level)
    {
        var levelSwitch = ValidateLogLevelSwitch(name);
        if (levelSwitch == null || !ModelState.IsValid)
        {
            return ValidationProblem();
        }

        levelSwitch.MinimumLevel = level;

        return NoContent();
    }

    LoggingLevelSwitch? ValidateLogLevelSwitch(string name)
    {
        if (!_logLevelSwitches.TryGetValue(name, out var levelSwitch))
        {
            var error = $"There is no log level switch named '{name}'. " +
                        $"The available names are: '{string.Join("', '", _logLevelSwitches.Keys)}'";
            ModelState.AddModelError(nameof(name), error);
        }

        return levelSwitch;
    }

    [HttpGet("filters")]
    public IDictionary<string, string?> GetLogFilterSwitches()
    {
        return _logFilterSwitches.ToDictionary(e => e.Key, e => e.Value.Expression);
    }

    [HttpPost("filters")]
    public IActionResult SetLogFilterSwitches([Required] string name, [Required] string expression)
    {
        var filterSwitch = ValidateLogFilterSwitch(name, expression);
        if (filterSwitch == null || !ModelState.IsValid)
        {
            return ValidationProblem();
        }

        filterSwitch.Expression = expression;

        return NoContent();
    }

    ILoggingFilterSwitch? ValidateLogFilterSwitch([Required] string name, [Required] string expression)
    {
        if (!_logFilterSwitches.TryGetValue(name, out var filterSwitch))
        {
            var error = $"There is no log filter switch named '{name}'. " +
                        $"The available names are: '{string.Join("', '", _logFilterSwitches.Keys)}'";
            ModelState.AddModelError(nameof(name), error);
        }

        if (!SerilogExpression.TryCompile(expression, out _, out var expressionError))
        {
            ModelState.AddModelError(nameof(expression), expressionError);
        }

        return filterSwitch;
    }

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
