namespace ReelShort.Domain.Entities;

public class VideoHashtag
{
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;

    public Guid HashtagId { get; set; }
    public Hashtag Hashtag { get; set; } = null!;
}