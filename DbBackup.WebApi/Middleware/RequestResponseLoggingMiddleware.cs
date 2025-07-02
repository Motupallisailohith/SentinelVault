using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DbBackup.WebApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        try
        {
            // Log request
            var request = $"{context.Request.Method} {context.Request.Path}";
            if (context.Request.HasJsonContentType())
            {
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                _logger.LogInformation($"Request: {request} - Body: {requestBody}");
            }
            else
            {
                _logger.LogInformation($"Request: {request}");
            }

            // Continue processing
            await _next(context);

            // Log response
            newBody.Seek(0, SeekOrigin.Begin);
            var responseString = await new StreamReader(newBody).ReadToEndAsync();
            _logger.LogInformation($"Response: {responseString}");

            newBody.Seek(0, SeekOrigin.Begin);
            await newBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}

// Extension method to add middleware
public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
