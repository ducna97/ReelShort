using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ReelShort.API.Middlewares;
using ReelShort.Domain.Common;

namespace ReelShort.API.Controllers;

[ApiController]
public abstract class BaseControllerAPI : ControllerBase
{
    private ILogger? _logger;
    
    /// <summary>
    /// Logger instance - automatically resolves according to the child controller's type
    /// </summary>
    protected ILogger Logger => _logger ??= HttpContext.RequestServices .GetRequiredService<ILoggerFactory>()
        .CreateLogger(GetType());
    
    /// <summary>
    /// Get the ID of the currently logged in user from JWT Token
    /// </summary>
    protected Guid CurrentUserId
    {
        get
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }
    
    /// <summary>
    /// Get the Username of the currently logged in user from JWT Token
    /// </summary>
    protected string CurrentUsername => User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    
    protected void InitCreateEntity(BaseEntity entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CreatedBy = CurrentUsername;
        entity.UpdatedBy = CurrentUsername;
    }

    protected void InitUpdateEntity(BaseEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentUsername;
    }
    
    protected IActionResult SuccessResponse<T>(T data, string message = "Success")
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data
        });
    }

    protected IActionResult CreatedResponse<T>(T data, string message = "Created successfully")
    {
        return StatusCode(201, new ApiResponse<T>
        {
            Success = true,
            StatusCode = 201,
            Message = message,
            Data = data
        });
    }

    protected IActionResult ErrorResponse(int statusCode, string message, List<string>? errors = null)
    {
        return StatusCode(statusCode, new ApiResponse<object>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors
        });
    }

    protected IActionResult NotFoundResponse(string message = "Resource not found")
    {
        return ErrorResponse(404, message);
    }

    protected IActionResult BadRequestResponse(string message, List<string>? errors = null)
    {
        return ErrorResponse(400, message, errors);
    }
}