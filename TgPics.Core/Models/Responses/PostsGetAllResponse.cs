using System.Text.Json.Serialization;

namespace TgPics.Core.Models.Responses;

public class PostsGetAllResponse
{
    public PostsGetAllResponse(int count, List<PostModel> posts)
    {
        Count = count;
        Items = posts;
    }

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("items")]
    public List<PostModel> Items { get; set; }
}