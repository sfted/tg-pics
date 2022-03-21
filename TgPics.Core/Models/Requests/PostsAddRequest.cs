using System.Text.Json.Serialization;
using TgPics.Core.Entities;

namespace TgPics.Core.Models.Requests;

public class PostsAddRequest
{
    public PostsAddRequest(
        List<int> mediaIds, 
        string sourceLink,
        string sourcePlatform,
        string sourceTitle,
        string? inviteLink = null,
        string? comment = null)
    {
        MediaIds = mediaIds;
        SourceLink = sourceLink;
        SourcePlatfrom = sourcePlatform;
        SourceTitle = sourceTitle;
        InviteLink = inviteLink;
        Comment = comment;
    }

    [JsonPropertyName("source_link")]
    public string SourceLink { get; set; }

    [JsonPropertyName("source_platform")]
    public string SourcePlatfrom { get; set; }

    [JsonPropertyName("source_title")]
    public string SourceTitle { get; set; }

    [JsonPropertyName("invite_link")]
    public string? InviteLink { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("media_ids")]
    public List<int> MediaIds { get; set; }
}