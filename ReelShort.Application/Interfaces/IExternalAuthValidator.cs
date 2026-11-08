using ReelShort.Application.DTOs.Auth;
using ReelShort.Domain.Enums;

namespace ReelShort.Application.Interfaces;

public interface IExternalAuthValidator
{
    Task<ValidatedExternalIdentity> ValidateAsync(ExternalAuthProvider provider, string accessToken, CancellationToken cancellationToken = default);
}
