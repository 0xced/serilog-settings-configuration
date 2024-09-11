using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Serilog;

// See https://github.com/serilog/serilog-settings-configuration/issues/321#issuecomment-2344034460

var fileSource = new MemoryConfigurationSource
{
    InitialData = new Dictionary<string, string?>
    {
        ["Serilog:WriteTo:ConsoleSink:Name"] = "Console",
        ["Serilog:WriteTo:ConsoleSink:Args:OutputTemplate"] = "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}    {Message:lj}{NewLine}{Exception}",
    },
};
var envSource = new MemoryConfigurationSource
{
    InitialData = new Dictionary<string, string?>
    {
#if false
        ["Serilog:WriteTo:ConsoleSink:Args:OutputTemplate"] = null, // crashes with System.ArgumentNullException: Value cannot be null. (Parameter 'outputTemplate')
#elif false
        ["Serilog:WriteTo:ConsoleSink:Args:OutputTemplate"] = "", // => uses the empty output template
#endif
        ["Serilog:WriteTo:ConsoleSink:Args:Formatter"] = "Serilog.Formatting.Compact.RenderedCompactJsonFormatter, Serilog.Formatting.Compact",
    },
};

Serilog.Debugging.SelfLog.Enable(Console.Out);

var configuration = new ConfigurationBuilder()
    .Add(fileSource)
    .Add(envSource)
    .Build();

Console.WriteLine($"BEFORE: {configuration.GetDebugView()}");
if (args is ["remove"])
{
    var removedProviders = configuration.RemoveSection("Serilog:WriteTo:ConsoleSink:Args:OutputTemplate").ToList();
    Console.WriteLine($"AFTER: {configuration.GetDebugView()}");
    Console.WriteLine($"Removed {removedProviders.Count} provider(s): {string.Join(", ", removedProviders)}");
}

using var logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

logger.Information("Hello");

Console.WriteLine("Done");
return 0;

static class ConfigurationExtensions
{
    public static IEnumerable<ConfigurationProvider> RemoveSection(this IConfigurationRoot configurationRoot, string section)
    {
        return configurationRoot.Providers.OfType<ConfigurationProvider>().Where(configurationProvider => configurationProvider.RemoveSection(section));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Data")]
    static extern IDictionary<string, string?> get_Data(ConfigurationProvider configurationProvider);

    static bool RemoveSection(this ConfigurationProvider configurationProvider, string section)
    {
        var data = get_Data(configurationProvider);
        return data.Remove(section);
    }
}
