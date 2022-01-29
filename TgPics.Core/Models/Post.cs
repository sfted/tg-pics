namespace TgPics.Core.Models;

public class Post
{
    public Post() { }
    public int Id { get; set; }
    public string SourceLink { get; set; }
    public string SourceTitle { get; set; }
    public DateTime Time { get; set; }
    public string? Text { get; set; }
    public IEnumerable<Picture> Pictures { get; set; }
}
