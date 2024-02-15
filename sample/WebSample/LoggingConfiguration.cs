using System.Net.Sockets;
using Serilog;
using Serilog.Debugging;
using Serilog.Expressions;
using Serilog.Formatting.Log4Net;
using Serilog.Settings.Configuration;
using Serilog.Sinks.Udp.Private;

namespace WebSample;

static class LoggingConfiguration
{
    static LoggingConfiguration()
    {
        SelfLog.Enable(text => Console.WriteLine("⚠️ Serilog internal error: " + text));
    }

    public static IServiceCollection AddLogSwitchesAccessor(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<ILogSwitchesAccessor, LogSwitchesAccessor>();
        return services;
    }

    public static void ConfigureLogger(HostBuilderContext context, IServiceProvider serviceProvider, LoggerConfiguration configuration)
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
        var accessor = (LogSwitchesAccessor)serviceProvider.GetRequiredService<ILogSwitchesAccessor>();
        var readerOptions = new ConfigurationReaderOptions(configurationAssemblies)
        {
            OnLevelSwitchCreated = (switchName, levelSwitch) => accessor.LogLevelSwitches[switchName] = levelSwitch,
            OnFilterSwitchCreated = (switchName, filterSwitch) => accessor.LogFilterSwitches[switchName] = filterSwitch,
        };
        configuration.ReadFrom.Configuration(context.Configuration, readerOptions);
        configuration.Filter.ByExcluding(e => e.Exception is OperationCanceledException);
        configuration.WriteTo.Sink(new HttpResponseSink("/logs/test", serviceProvider.GetRequiredService<IHttpContextAccessor>()));
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
}
