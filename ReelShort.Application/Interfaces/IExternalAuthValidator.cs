using ReelShort.Application.DTOs.Auth;

namespace ReelShort.Application.Interfaces;

public interface IExternalAuthValidator
{
    Task<ValidatedExternalIdentity> ValidateAsync(string provider, string accessToken, CancellationToken cancellationToken = default);
}
