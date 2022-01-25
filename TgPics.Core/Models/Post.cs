namespace TgPics.Core.Models;

public class Post
{
    public Post() { }

    public int Id { get; set; }

    public string Text { get; set; }

    public IEnumerable<Picture> Pictures { get; set; }

    public string Source { get; set; }

    public DateTime Time { get; set; }
}
