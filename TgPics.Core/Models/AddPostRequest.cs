namespace TgPics.Core.Models;

public class AddPostRequest
{
    public string SourceLink { get; set; }
    public string SourceTitle { get; set; }
    public string? Comment { get; set; }
    public IEnumerable<PictureInfo> Pictures { get; set; }
}

public class PictureInfo
{
    public string Url { get; set; }
    public int Position { get; set; }
}