namespace TgPics.Core.Models;

public class PrepareToPublishPost
{
    public Uri SourceLink { get; set; }
    public string SourceTitle { get; set; }
    public List<PrepareToPublishPhoto> Photos { get; set; }
}

public class PrepareToPublishPhoto
{
    public Uri OriginalUrl { get; set; }
    public Uri Preview32Url { get; set; }
}