using System.Net.Http.Headers;
using System.Net.Http.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReelShort.Application.Common.Configs;
using ReelShort.Application.Common.Exceptions;
using ReelShort.Application.DTOs.Auth;
using ReelShort.Application.Interfaces;
using ReelShort.Domain.Enums;
using FacebookDebugTokenResponseModel = ReelShort.Infrastructure.Models.Facebook.FacebookDebugTokenResponse;
using FacebookUserInfoModel = ReelShort.Infrastructure.Models.Facebook.FacebookUserInfo;

namespace ReelShort.Infrastructure.Services;

public sealed class ExternalAuthValidator : IExternalAuthValidator
{
    private readonly HttpClient _httpClient;
    private readonly ExternalLoginSettings _settings;
    private readonly ILogger<ExternalAuthValidator> _logger;

    public ExternalAuthValidator(
        HttpClient httpClient,
        IOptions<ExternalLoginSettings> settings,
        ILogger<ExternalAuthValidator> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public Task<ValidatedExternalIdentity> ValidateAsync(ExternalAuthProvider provider,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ExternalAuthException("Thiếu thông tin xác thực từ nhà cung cấp.");
        }

        return provider switch
        {
            ExternalAuthProvider.Google => ValidateGoogleAsync(accessToken),
            ExternalAuthProvider.Facebook => ValidateFacebookAsync(accessToken, cancellationToken),
            _ => throw new ExternalAuthException("Nhà cung cấp đăng nhập không được hỗ trợ.")
        };
    }

    private async Task<ValidatedExternalIdentity> ValidateGoogleAsync(string token)
    {
        var clientId = _settings.Google.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ExternalAuthException("Đăng nhập Google chưa được cấu hình.", 503);
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            });

            if (string.IsNullOrWhiteSpace(payload.Subject)
                || string.IsNullOrWhiteSpace(payload.Email)
                || !payload.EmailVerified)
            {
                throw new ExternalAuthException("Tài khoản Google không cung cấp email đã xác minh.");
            }

            return new ValidatedExternalIdentity("Google", payload.Subject, payload.Email.Trim().ToLowerInvariant(),
                string.IsNullOrWhiteSpace(payload.Name) ? payload.Email : payload.Name, payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning("Google credential validation failed with {FailureType}.", ex.GetType().Name);
            throw new ExternalAuthException("Thông tin xác thực Google không hợp lệ hoặc đã hết hạn.");
        }
    }

    private async Task<ValidatedExternalIdentity> ValidateFacebookAsync(
        string token,
        CancellationToken cancellationToken)
    {
        var facebook = _settings.Facebook;
        if (string.IsNullOrWhiteSpace(facebook.AppId) || string.IsNullOrWhiteSpace(facebook.AppSecret))
        {
            throw new ExternalAuthException("Đăng nhập Facebook chưa được cấu hình.", 503);
        }

        var appToken = Uri.EscapeDataString($"{facebook.AppId}|{facebook.AppSecret}");
        var inputToken = Uri.EscapeDataString(token);
        var version = string.IsNullOrWhiteSpace(facebook.GraphApiVersion)
            ? "v26.0"
            : facebook.GraphApiVersion.Trim('/');

        try
        {
            var debug = await _httpClient.GetFromJsonAsync<FacebookDebugTokenResponseModel>(
                $"{version}/debug_token?input_token={inputToken}&access_token={appToken}",
                cancellationToken);
            var data = debug?.Data;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (data is null
                || !data.IsValid
                || !string.Equals(data.AppId, facebook.AppId, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(data.UserId)
                || IsExpired(data.ExpiresAt, now)
                || IsExpired(data.DataAccessExpiresAt, now))
            {
                throw new ExternalAuthException("Thông tin xác thực Facebook không hợp lệ hoặc đã hết hạn.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{version}/me?fields=id,name,email,picture.type(large)");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalAuthException("Không thể xác minh tài khoản Facebook.");
            }

            var profile = await response.Content.ReadFromJsonAsync<FacebookUserInfoModel>(cancellationToken);
            if (profile is null
                || !string.Equals(profile.Id, data.UserId, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(profile.Email))
            {
                throw new ExternalAuthException(
                    "Facebook không cung cấp email hợp lệ. Hãy cấp quyền email và thử lại.");
            }

            return new ValidatedExternalIdentity(
                "Facebook",
                data.UserId,
                profile.Email.Trim().ToLowerInvariant(),
                string.IsNullOrWhiteSpace(profile.Name) ? profile.Email : profile.Name,
                profile.Picture?.Data?.Url);
        }
        catch (ExternalAuthException)
        {
            throw;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExternalAuthException("Facebook phản hồi quá chậm. Vui lòng thử lại.", 503);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                "Facebook validation request failed with {FailureType}.",
                ex.GetType().Name);
            throw new ExternalAuthException("Không thể kết nối tới Facebook. Vui lòng thử lại.", 503);
        }
    }

    private static bool IsExpired(long value, long now) => value > 0 && value <= now;
}
