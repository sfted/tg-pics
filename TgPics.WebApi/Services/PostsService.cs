using TgPics.Core.Models;

namespace TgPics.WebApi.Services;

public interface IPostsService
{
    public void Add(Post post);
    public void Remove(Post post);
    public List<Post> GetAll();
}

public class PostsService: IPostsService
{
    public void Add(Post post)
    {
        using var database = new DBService();
        database.Posts.Add(post);
        database.SaveChanges();
    }

    public void Remove(Post post)
    {
        using var database = new DBService();
        if (database.Posts.Contains(post))
        {
            database.Posts.Remove(post);
            database.SaveChanges();
        }
    }

    public List<Post> GetAll()
    {
        using var database = new DBService();
        return database.Posts.ToList();
    }
}
