namespace TgPics.Core.Entities;

public class Post
{
    public int Id { get; set; }
    public string? SourceLink { get; set; }
    public string? SourceTitle { get; set; }
    public DateTime PublicationDateTime { get; set; }
    public string? Comment { get; set; }
    public IEnumerable<MediaFile>? Media { get; set; }
}
