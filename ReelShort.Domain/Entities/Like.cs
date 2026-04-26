namespace ReelShort.Domain.Entities;

public class Like
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}