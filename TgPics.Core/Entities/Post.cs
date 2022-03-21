namespace TgPics.Core.Entities;

public class Post
{
    public int Id { get; set; }
    public string SourceLink { get; set; }
    public string SourcePlatform { get; set; }
    public string SourceTitle { get; set; }
    public string? InviteLink {get;set;}
    public DateTime PublicationDateTime { get; set; }
    public string? Comment { get; set; }
    public List<MediaFile> Media { get; set; } = new();
    public bool? ManagedToPublish { get; set; }
    public string? PublicationError { get; set; }
}
