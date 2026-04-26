using ReelShort.Domain.Common;

namespace ReelShort.Domain.Entities;

public class Hashtag : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Example: #dance, #vlog
    public long ViewCount { get; set; } = 0; // Total views of all videos containing this hashtag

    public ICollection<VideoHashtag> VideoHashtags { get; set; } = new HashSet<VideoHashtag>();
}