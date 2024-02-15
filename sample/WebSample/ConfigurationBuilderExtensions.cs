using Tomlyn.Extensions.Configuration;

namespace WebSample;

public static class ConfigurationBuilderExtensions
{
    public static void UseToml(this IConfigurationBuilder configuration)
    {
        configuration.Use<TomlConfigurationSource>("toml");
    }

    static void Use<TSource>(this IConfigurationBuilder configuration, string pathExtension) where TSource : FileConfigurationSource, new()
    {
        for (var i = configuration.Sources.Count - 1; i >= 0; i--)
        {
            if (configuration.Sources[i] is FileConfigurationSource source)
            {
                configuration.Sources[i] = new TSource
                {
                    FileProvider = source.FileProvider,
                    Path = Path.ChangeExtension(source.Path ?? "", pathExtension),
                    Optional = source.Optional,
                    ReloadOnChange = source.ReloadOnChange,
                    ReloadDelay = source.ReloadDelay,
                    OnLoadException = source.OnLoadException,
                };
            }
        }
    }
}
