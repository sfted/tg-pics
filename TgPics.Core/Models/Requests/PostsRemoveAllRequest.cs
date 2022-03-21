using System.Text.Json.Serialization;

namespace TgPics.Core.Models.Requests;

public class PostsRemoveAllRequest
{
    public PostsRemoveAllRequest(string confirmation)
    {
        Confirmation = confirmation;
    }

    [JsonPropertyName("confirmation")]
    public string Confirmation { get; set; }
}