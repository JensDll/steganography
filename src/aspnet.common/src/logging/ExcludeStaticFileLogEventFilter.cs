using aspnet.common.extensions;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;

namespace aspnet.common.logging;

public class ExcludeStaticFileLogEventFilter : ILogEventFilter
{
    public bool IsEnabled(LogEvent logEvent)
    {
        return !Matching.FromSource("Microsoft.AspNetCore.Hosting.Diagnostics")(logEvent) ||
               Matching.WithProperty("Path", static (PathString path) => path.IsApi())(logEvent);
    }
}
