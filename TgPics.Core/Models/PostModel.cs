namespace TgPics.Core.Models;

using System.Text.Json.Serialization;
using TgPics.Core.Entities;

public class PostModel
{
    public PostModel(string host, Post post)
    {
        Id = post.Id;
        SourceLink = post.SourceLink;
        SourcePlatfrom = post.SourcePlatform;
        SourceTitle = post.SourceTitle;
        InviteLink = post.InviteLink;
        Comment = post.Comment;
        PublicationDateTime = post.PublicationDateTime;
        ManagedToPublish = post.ManagedToPublish;
        PublicationError = post.PublicationError;

        Media = post.Media?
            .Select(p => new MediaFileInfo(host, p)) 
            ?? new List<MediaFileInfo>();
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }

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

    [JsonPropertyName("has_comment")]
    public bool HasComment => !string.IsNullOrEmpty(Comment);

    [JsonPropertyName("has_invite_link")]
    public bool HasInviteLink => !string.IsNullOrEmpty(InviteLink);

    [JsonPropertyName("publication_date_time")]
    public DateTime PublicationDateTime { get; set; }

    [JsonPropertyName("managed_to_publish")]
    public bool? ManagedToPublish { get; set; }

    [JsonPropertyName("publication_error")]
    public string? PublicationError { get; set; }

    [JsonPropertyName("media")]
    public IEnumerable<MediaFileInfo> Media { get; set; }
}