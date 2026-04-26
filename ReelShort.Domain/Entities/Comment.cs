using ReelShort.Domain.Common;

namespace ReelShort.Domain.Entities;

public class Comment : BaseEntity
{
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? ParentCommentId { get; set; } // Used for Reply feature
    public Comment? ParentComment { get; set; }
    
    public string Content { get; set; } = string.Empty;
    public int LikeCount { get; set; } = 0;

    // Navigation Properties
    public ICollection<Comment> Replies { get; set; } = new HashSet<Comment>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new HashSet<CommentLike>();
}
