using System.Text.Json.Serialization;

namespace TgPics.Core.Models.Requests;

public class IdRequest
{
    public IdRequest(int id)
    { 
        Id = id;
    }

    [JsonPropertyName("id")]
    public int Id { get; set; }
}