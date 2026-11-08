using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReelShort.Application.Common.Configs;
using ReelShort.Application.Common.Exceptions;
using ReelShort.Application.DTOs.Auth;
using ReelShort.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace ReelShort.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseControllerAPI
{
    private readonly IAuthService _authService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalLoginSettings _loginSettings;
    
    public AuthController(IAuthService authService,
        IHttpClientFactory httpClientFactory,
        IOptions<ExternalLoginSettings> loginSettings)
    {
        _authService = authService;
        _httpClientFactory = httpClientFactory;
        _loginSettings = loginSettings.Value;
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
        try
        {
            Logger.LogInformation($"User login attempt, email: {request.Email}.");
            var result = await _authService.LoginAsync(request);
            Logger.LogInformation($"User login successful, email: {request.Email}.");
            return SuccessResponse(result, "Login successful");
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning(ex, "Password login is not available for email: {Email}.", request.Email);
            return BadRequestResponse("Tài khoản này chưa có mật khẩu nội bộ hoặc được tạo bằng đăng nhập ngoài.", [ex.Message]);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Login failed for email: {Email}.", request.Email);
            return BadRequestResponse("Email hoặc mật khẩu không đúng.", [ex.Message]);
        }
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest? request = null)
    {
        Logger.LogInformation("User logout attempt.");

        var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequestResponse("Access token is required for logout");
        }

        await _authService.LogoutAsync(accessToken, CurrentUserId, request?.RefreshToken);

        Logger.LogInformation("User logout successful.");
        return SuccessResponse<object?>(null, "Logout successful");
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] ExternalAuthRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation("Google login attempt.");

            var validator = HttpContext.RequestServices.GetRequiredService<IExternalAuthValidator>();
            var identity = await validator.ValidateAsync("Google", request.AccessToken, cancellationToken);
            var result = await _authService.ExternalLoginAsync(identity);

            Logger.LogInformation("Google login successful for user {UserId}.", result.Id);
            return SuccessResponse(result, "Google login successful");
        }
        catch (ExternalAuthException ex)
        {
            Logger.LogWarning("Google login rejected: {Reason}", ex.Message);
            return ErrorResponse(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Google login failed.");
            return ErrorResponse(500, "Không thể đăng nhập bằng Google. Vui lòng thử lại.");
        }
    }
    
    [HttpPost("facebook-login")]
    public async Task<IActionResult> FacebookLogin(
        [FromBody] ExternalAuthRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation("Facebook login attempt.");

            var validator = HttpContext.RequestServices.GetRequiredService<IExternalAuthValidator>();
            var identity = await validator.ValidateAsync("Facebook", request.AccessToken, cancellationToken);
            var result = await _authService.ExternalLoginAsync(identity);

            Logger.LogInformation("Facebook login successful for user {UserId}.", result.Id);
            return SuccessResponse(result, "Facebook login successful");
        }
        catch (ExternalAuthException ex)
        {
            Logger.LogWarning("Facebook login rejected: {Reason}", ex.Message);
            return ErrorResponse(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Facebook login failed.");
            return ErrorResponse(500, "Không thể đăng nhập bằng Facebook. Vui lòng thử lại.");
        }
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        Logger.LogInformation("Refresh token attempt.");

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequestResponse("Refresh token is required.");
        }

        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        Logger.LogInformation("Refresh token successful.");
        return SuccessResponse(result, "Refresh token successful");
    }
}
