using TgPics.Core.Enums;

namespace TgPics.Core.Entities;

public class MediaFile
{
    public int Id { get; set; }
    public MediaType Type { get; set; }
    public Uri? FileName { get; set; }

    public int? PostId { get; set; }
    public Post? Post { get; set; }
}