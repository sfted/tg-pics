namespace TgPics.Core.Models;

using TgPics.Core.Entities;

public class GetAllPostsResponse
{
    public List<Post> Posts { get; set; }

    //TODO: paging etc
}