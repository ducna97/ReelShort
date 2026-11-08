namespace ReelShort.Infrastructure.Models.Facebook;

internal sealed class FacebookUserInfo
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public FacebookPicture? Picture { get; init; }
}
