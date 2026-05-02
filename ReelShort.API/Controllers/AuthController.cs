using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReelShort.Application.DTOs.Auth;
using ReelShort.Application.Interfaces;

namespace ReelShort.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseControllerAPI
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        Logger.LogInformation($"Registering new user, username: {request.Username}, email: {request.Email}.");
        var result = await _authService.RegisterAsync(request);
        Logger.LogInformation($"Registering user successful, username: {request.Username}, email: {request.Email}.");
        return CreatedResponse(result, "Registration successful");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        Logger.LogInformation($"User login attempt, email: {request.Email}.");
        var result = await _authService.LoginAsync(request);
        Logger.LogInformation($"User login successful, email: {request.Email}.");
        return SuccessResponse(result, "Login successful");
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        Logger.LogInformation("User logout attempt.");
        var token = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            return BadRequestResponse("Token is required for logout");
        }
        
        await _authService.LogoutAsync(token);
        Logger.LogInformation("User logout successful.");
        return SuccessResponse<object?>(null, "Logout successful");
    }
}