using System.Net;
using ILogger = Serilog.ILogger;

namespace WebApi.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Method != HttpMethod.Get.Method)
        {
            IPAddress? remoteIp = context.Connection.RemoteIpAddress;
            _logger.Debug("The request remote IP address is: {RemoteIp}", remoteIp);
        }

        await _next.Invoke(context);
    }
}
