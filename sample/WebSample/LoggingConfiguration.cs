using System.Collections.Frozen;
using System.Net.Sockets;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Expressions;
using Serilog.Formatting.Log4Net;
using Serilog.Settings.Configuration;
using Serilog.Sinks.Udp.Private;

namespace WebSample;

class LoggingConfiguration : ILogSwitchesAccessor
{
    FrozenDictionary<string, LoggingLevelSwitch> _logLevelSwitches = FrozenDictionary<string, LoggingLevelSwitch>.Empty;
    FrozenDictionary<string, ILoggingFilterSwitch> _logFilterSwitches = FrozenDictionary<string, ILoggingFilterSwitch>.Empty;

    static LoggingConfiguration()
    {
        SelfLog.Enable(text => Console.WriteLine("⚠️ Serilog internal error: " + text));
    }

    public static IServiceCollection AddLogSwitchesAccessor(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<LoggingConfiguration>();
        services.AddSingleton<ILogSwitchesAccessor>(sp => sp.GetRequiredService<LoggingConfiguration>());
        return services;
    }

    internal void ConfigureLogger(HostBuilderContext context, LoggerConfiguration configuration)
    {
        var configurationAssemblies = new[]
        {
            typeof(ILogger).Assembly,                               // Serilog
            typeof(SerilogExpression).Assembly,                     // Serilog.Expressions
            typeof(ProcessLoggerConfigurationExtensions).Assembly,  // Serilog.Enrichers.Process
            typeof(ThreadLoggerConfigurationExtensions).Assembly,   // Serilog.Enrichers.Thread
            typeof(Log4NetTextFormatter).Assembly,                  // Serilog.Formatting.Log4Net
            typeof(ConsoleLoggerConfigurationExtensions).Assembly,  // Serilog.Sinks.Console
            typeof(LoggerConfigurationEventLogExtensions).Assembly, // Serilog.Sinks.EventLog
            typeof(FileLoggerConfigurationExtensions).Assembly,     // Serilog.Sinks.File
            typeof(NotepadLoggerConfigurationExtensions).Assembly,  // Serilog.Sinks.Notepad
            typeof(UdpClientFactory).Assembly,                      // Serilog.Sinks.Udp
        };

        var logLevelSwitches = new Dictionary<string, LoggingLevelSwitch>();
        var logFilterSwitches = new Dictionary<string, ILoggingFilterSwitch>();
        var readerOptions = new ConfigurationReaderOptions(configurationAssemblies)
        {
            OnLevelSwitchCreated = logLevelSwitches.Add,
            OnFilterSwitchCreated = logFilterSwitches.Add,
        };
        configuration.ReadFrom.Configuration(context.Configuration, readerOptions);

        _logLevelSwitches = logLevelSwitches.ToFrozenDictionary();
        _logFilterSwitches = logFilterSwitches.ToFrozenDictionary();
    }

    public static ILogger CreateBootstrapLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .WriteTo.Console()
            .WriteTo.Udp("localhost", 7071, AddressFamily.InterNetwork, Log4NetTextFormatter.Log4JFormatter)
            .WriteTo.Notepad()
            .CreateBootstrapLogger();
    }

    IReadOnlyDictionary<string, LoggingLevelSwitch> ILogSwitchesAccessor.LogLevelSwitches => _logLevelSwitches;
    IReadOnlyDictionary<string, ILoggingFilterSwitch> ILogSwitchesAccessor.LogFilterSwitches => _logFilterSwitches;
}
