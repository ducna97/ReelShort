using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ReelShort.API.Middlewares;

public class RequestResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseMiddleware> _logger;
    
    public RequestResponseMiddleware(RequestDelegate next, ILogger<RequestResponseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        // --- LOG REQUEST ---
        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;
        _logger.LogInformation("[{RequestId}] ▶ {Method} {Path}", requestId, method, path);
        
        // Read request body (if any)
        context.Request.EnableBuffering();
        if (context.Request.ContentLength > 0)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                _logger.LogInformation("[{RequestId}] Body: {Body}", requestId, body);
            }
        }
        
        // --- INTERCEPT RESPONSE ---
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;

            // Read response body
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation("[{RequestId}] ◀ {StatusCode} ({Elapsed}ms)",
                requestId, statusCode, stopwatch.ElapsedMilliseconds);

            // Copy the response back to the client
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "[{RequestId}] ✖ Exception ({Elapsed}ms): {Message}",
                requestId, stopwatch.ElapsedMilliseconds, ex.Message);

            // Returns standard error response
            context.Response.Body = originalBodyStream;
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var errorResponse = new ApiResponse<object>
            {
                Success = false,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred.",
                Errors = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                    ? [ex.Message, ex.StackTrace ?? ""]
                    : null
            };

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}