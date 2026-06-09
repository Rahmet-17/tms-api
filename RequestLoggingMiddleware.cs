using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        var stopwatch = Stopwatch.StartNew();

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        _logger.LogInformation(
            "START {Method} {Path} [{CorrelationId}]",
            context.Request.Method,
            context.Request.Path,
            correlationId
        );

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "END {StatusCode} {ElapsedMs}ms [{CorrelationId}]",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId
        );
    }
}