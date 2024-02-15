using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Rewrite;
using Serilog;
using WebSample;
using static WebSample.LoggingConfiguration;

try
{
    Log.Logger = CreateBootstrapLogger();

    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.UseToml();
    builder.Host.UseSerilog(ConfigureLogger);

    builder.Services
        .AddLogSwitchesAccessor()
        .AddHttpContextAccessor()
        .AddSwaggerGen()
        .AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    var webApp = builder.Build();
    var rewriteOptions = new RewriteOptions();
    rewriteOptions.AddRewrite("^$", "swagger", skipRemainingRules: true);
    webApp
        .UseRewriter(rewriteOptions)
        .UseSerilogRequestLogging()
        .UseSwagger()
        .UseSwaggerUI(options => options.EnableTryItOutByDefault());
    webApp.MapControllers();

    Log.Information("Starting the web application in {CurrentDirectory}", Environment.CurrentDirectory);
    webApp.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "An unhandled exception has occurred");
}
finally
{
    Log.CloseAndFlush();
}
