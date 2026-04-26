namespace ReelShort.Domain.Entities;

public class Follow
{
    public Guid FollowerId { get; set; }
    public User Follower { get; set; } = null!;
    
    public Guid FolloweeId { get; set; }
    public User Followee { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}