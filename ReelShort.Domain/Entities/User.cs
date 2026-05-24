using ReelShort.Domain.Common;

namespace ReelShort.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    
    // Denormalized Fields
    public int FollowerCount { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;
    public int TotalLikes { get; set; } = 0;
    
    // Navigation properties
    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Followings { get; set; } = new List<Follow>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
