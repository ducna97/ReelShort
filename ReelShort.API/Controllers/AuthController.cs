using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReelShort.Application.Common.Configs;
using ReelShort.Application.DTOs.Auth;
using ReelShort.Application.Interfaces;

namespace ReelShort.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseControllerAPI
{
    private readonly IAuthService _authService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ExternalLoginSettings _loginSettings;
    
    public AuthController(IAuthService authService,
        IHttpClientFactory httpClientFactory,
        ExternalLoginSettings loginSettings)
    {
        _authService = authService;
        _httpClientFactory = httpClientFactory;
        _loginSettings = loginSettings;
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
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest? request = null)
    {
        Logger.LogInformation("User logout attempt.");

        var accessToken = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequestResponse("Access token is required for logout");
        }

        await _authService.LogoutAsync(accessToken, request?.RefreshToken);

        Logger.LogInformation("User logout successful.");
        return SuccessResponse<object?>(null, "Logout successful");
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] ExternalAuthRequest request)
    {
        try
        {
            Logger.LogInformation("Google login attempt.");
            
            var googleClientId = _loginSettings.Google.ClientId;
            if (string.IsNullOrEmpty(googleClientId))
            {
                Logger.LogError("Google client id is null or empty.");
                return ErrorResponse(500, "Google ClientId is not configured.");
            }
            
            // Verify Google ID token
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { googleClientId },
            };
            
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.AccessToken, settings);
            
            var result = await _authService.ExternalLoginAsync(
                payload.Email,
                payload.Name,
                payload.Picture,
                "Google"
            );
            
            Logger.LogInformation($"Google login successful for email: {payload.Email}.");
            return SuccessResponse(result, "Google login successful");
        }
        catch (InvalidJwtException ex)
        {
            Logger.LogWarning($"Invalid Google token, error: {ex.Message}");
            return BadRequestResponse("Invalid Google token.");
        }
    }
    
    [HttpPost("facebook-login")]
    public async Task<IActionResult> FacebookLogin([FromBody] ExternalAuthRequest request)
    {
        try
        {
            Logger.LogInformation("Facebook login attempt.");

            var appId = _loginSettings.Facebook.AppId;
            var appSecret = _loginSettings.Facebook.AppSecret;

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                Logger.LogError("Facebook App credentials are not configured.");
                return ErrorResponse(500, "Facebook App credentials are not configured.");
            }

            var httpClient = _httpClientFactory.CreateClient();

            // Step 1: Verify token with Facebook Debug Token API
            var debugTokenUrl = $"https://graph.facebook.com/debug_token?input_token={request.AccessToken}&access_token={appId}|{appSecret}";
            var debugResponse = await httpClient.GetFromJsonAsync<FacebookDebugTokenResponse>(debugTokenUrl);

            if (debugResponse?.Data == null || !debugResponse.Data.IsValid)
            {
                Logger.LogWarning("Invalid Facebook token.");
                return BadRequestResponse("Invalid Facebook token.");
            }

            // Step 2: Get user info from Facebook Graph API
            var userInfoUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture.type(large)&access_token={request.AccessToken}";
            var fbUser = await httpClient.GetFromJsonAsync<FacebookUserInfo>(userInfoUrl);

            if (fbUser == null || string.IsNullOrEmpty(fbUser.Email))
            {
                Logger.LogWarning("Could not retrieve Facebook user info or email not granted.");
                return BadRequestResponse("Could not retrieve Facebook user info. Make sure email permission is granted.");
            }

            var result = await _authService.ExternalLoginAsync(
                fbUser.Email,
                fbUser.Name ?? "Facebook User",
                fbUser.Picture?.Data?.Url,
                "Facebook"
            );

            Logger.LogInformation($"Facebook login successful for email: {fbUser.Email}.");
            return SuccessResponse(result, "Facebook login successful");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Facebook login failed.");
            return ErrorResponse(500, "Facebook login failed.");
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