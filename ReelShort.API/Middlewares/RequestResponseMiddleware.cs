using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ReelShort.API.Middlewares;

public class RequestResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseMiddleware> _logger;
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "confirmPassword",
        "token",
        "accessToken",
        "refreshToken",
        "idToken",
        "authorization"
    };
    
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
        var isSensitiveAuthPath = context.Request.Path.StartsWithSegments("/api/auth");
        _logger.LogInformation("[{RequestId}] ▶ {Method} {Path}", requestId, method, path);
        
        // Read request body (if any)
        context.Request.EnableBuffering();
        if (!isSensitiveAuthPath && context.Request.ContentLength > 0)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                _logger.LogInformation("[{RequestId}] Body: {Body}", requestId, SanitizePayload(body));
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
            if (!isSensitiveAuthPath && !string.IsNullOrWhiteSpace(responseText))
            {
                _logger.LogInformation("[{RequestId}] Response: {Body}", requestId, SanitizePayload(responseText));
            }

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

    private static string SanitizePayload(string payload)
    {
        try
        {
            var node = JsonNode.Parse(payload);
            if (node == null)
            {
                return payload;
            }

            RedactNode(node);
            return node.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = false
            });
        }
        catch (JsonException)
        {
            return payload;
        }
    }

    private static void RedactNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var item in jsonObject.ToList())
            {
                if (item.Value == null)
                {
                    continue;
                }

                if (SensitiveFields.Contains(item.Key))
                {
                    jsonObject[item.Key] = "***REDACTED***";
                    continue;
                }

                RedactNode(item.Value);
            }

            return;
        }

        if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item != null)
                {
                    RedactNode(item);
                }
            }
        }
    }
}
