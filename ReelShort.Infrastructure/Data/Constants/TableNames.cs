namespace ReelShort.Infrastructure.Data.Constants;

public static class TableNames
{
    public const string Users = "Users";
    public const string Videos = "Videos";
    public const string Musics = "Musics";
    public const string Comments = "Comments";
    public const string Likes = "Likes";
    public const string Follows = "Follows";
    public const string Hashtags = "Hashtags";
    public const string VideoHashtags = "VideoHashtags";
    public const string CommentLikes = "CommentLikes";
}

public static class ColumnLengths
{
    public const int MaxUsernameLength = 100;
    public const int MaxEmailLength = 150;
    public const int MaxNameLength = 255;
    public const int MaxCaptionLength = 2200;
}