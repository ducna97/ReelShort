using ReelShort.Domain.Common;

namespace ReelShort.Domain.Entities;

public class Music : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public int Duration { get; set; } // Measured in seconds
    public int UseCount { get; set; } = 0;

    public ICollection<Video> Videos { get; set; } = new HashSet<Video>();
}