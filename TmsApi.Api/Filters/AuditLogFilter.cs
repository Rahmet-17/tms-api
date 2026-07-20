using Microsoft.AspNetCore.Mvc.Filters;

namespace TmsApi.Application.Filters;

public class AuditLogFilter(
    ILogger<AuditLogFilter> logger) : IActionFilter
{
    public void OnActionExecuting(
        ActionExecutingContext context)
    {
        var route = context.HttpContext.Request.Path;
        var method = context.HttpContext.Request.Method;

        logger.LogInformation(
            "TMS API call: {Method} {Route}",
            method,
            route);
    }


    public void OnActionExecuted(
        ActionExecutedContext context)
    {
        if (context.Exception != null)
        {
            logger.LogError(
                context.Exception,
                "TMS API failed");
            
            return;
        }


        logger.LogInformation(
            "TMS API response: {StatusCode}",
            context.HttpContext.Response.StatusCode);
    }
}