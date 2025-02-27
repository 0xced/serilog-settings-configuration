using Microsoft.AspNetCore.Http.Features;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace WebSample;

/// <summary>
/// A sink that writes to the current HTTP context.
/// </summary>
/// <remarks>Use this for demonstration purpose only. Logging synchronously to the current HTTP context is not a good idea.</remarks>
public class HttpResponseSink : ILogEventSink
{
    readonly IHttpContextAccessor _contextAccessor;
    readonly PathString _path;
    readonly MessageTemplateTextFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpResponseSink"/> class.
    /// </summary>
    /// <param name="path">The path to match against the current HTTP request path.</param>
    /// <param name="contextAccessor">The <see cref="IHttpContextAccessor"/> that provides the current HTTP context.</param>
    public HttpResponseSink(PathString path, IHttpContextAccessor contextAccessor)
    {
        _path = path;
        _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        _formatter = new MessageTemplateTextFormatter("[{Level}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}");
    }

    void ILogEventSink.Emit(LogEvent logEvent)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null || !_path.Equals(httpContext.Request.Path))
            return;

        var bodyControl = httpContext.Features.Get<IHttpBodyControlFeature>() ?? throw new InvalidOperationException("IHttpBodyControlFeature is not available");
        bodyControl.AllowSynchronousIO = true;

        httpContext.Response.ContentType ??= "text/plain; charset=utf-8";
        using var output = new StreamWriter(httpContext.Response.Body);
        _formatter.Format(logEvent, output);
    }
}
