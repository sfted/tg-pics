using System.Text.Json.Serialization;

namespace TgPics.Core.Models.Responses;

public class MessageResponse
{
    public MessageResponse(string message)
    {
        Message = message;
    }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}