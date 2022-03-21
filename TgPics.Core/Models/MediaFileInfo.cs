namespace TgPics.Core.Models;

using System.Text.Json.Serialization;
using TgPics.Core.Entities;
using TgPics.Core.Enums;

public class MediaFileInfo
{
    public MediaFileInfo() { }

    public MediaFileInfo(string? host, MediaFile file)
    {
        Id = file.Id;
        Type = file.Type;
        file.Post = file.Post;

        if (string.IsNullOrEmpty(host))
            Url = null;
        else
            Url = new Uri($"{host}/api/files/get?id={Id}");
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public Uri? Url { get; set; }

    [JsonPropertyName("type")]
    public MediaType Type { get; set; }

    [JsonPropertyName("post")]
    public Post? Post { get; set; }
}