using ReelShort.Domain.Common;
using ReelShort.Domain.Enums;

namespace ReelShort.Domain.Entities;

public class Video : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? MusicId { get; set; } // Nullable: Video có thể không dùng nhạc nền
    public Music? Music { get; set; }

    public string VideoUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Caption { get; set; }
    public int Duration { get; set; }
    public VideoStatus Status { get; set; } = VideoStatus.Draft;

    // Denormalized Fields
    public long ViewCount { get; set; } = 0;
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;

    // Navigation Properties
    public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    public ICollection<Like> Likes { get; set; } = new HashSet<Like>();
    public ICollection<VideoHashtag> VideoHashtags { get; set; } = new HashSet<VideoHashtag>();
}