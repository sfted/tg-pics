using System.Text.Json.Serialization;

namespace TgPics.Core.Models.Responses;

public class FilesGetAllResponse
{
    public FilesGetAllResponse(int count, List<MediaFileInfo> items)
    {
        Count = count;
        Items = items;
    }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("items")]
    public List<MediaFileInfo> Items { get; set; }
}