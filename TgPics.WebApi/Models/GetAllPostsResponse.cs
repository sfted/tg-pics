namespace TgPics.WebApi.Models;

using TgPics.Core.Models;

public class GetAllPostsResponse
{
    public List<Post> Posts { get; set; }
    
    //TODO: paging etc
}