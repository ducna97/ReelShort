using System.Net;
using System.Text.Json;
using ReelShort.Application.Interfaces;

namespace ReelShort.API.Middlewares;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    
    public TokenBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenBlacklistService tokenBlacklistService)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token) && await tokenBlacklistService.IsBlacklistedAsync(token))
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            var response = new ApiResponse<object>
            {
                Success = false,
                StatusCode = context.Response.StatusCode,
                Message = "Token has been revoked. Please login again."
            };
            
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await context.Response.WriteAsync(json);
            return;
        }
        
        await _next(context);
    }
}