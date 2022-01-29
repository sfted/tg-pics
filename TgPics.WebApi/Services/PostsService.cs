using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgPics.Core.Models;
using TgPics.WebApi.Helpers;

namespace TgPics.WebApi.Services;

public interface IPostsService
{
    public void Add(Post post);
    public void Remove(Post post);
    public List<Post> GetAll();
    public void RemoveAll();
}

public class PostsService : IPostsService
{
    public PostsService(IOptions<AppSettings> settings)
    {
        this.settings = settings.Value;
    }

    private readonly AppSettings settings;

    public void Add(Post post)
    {
        using var database = new DBService();

        foreach (var pic in post.Pictures)
        {
            if (pic.Data == null && pic.Source != null)
                pic.Load();
        }

        if(post.Time == null)
        {
            if (database.Posts.Any())
            {
                var lastPost = database.Posts.OrderBy(p => p.Time).Last();
                var timing = lastPost.Time?.TimeOfDay;
                var nextTiming = settings.Timings.First(t => t > timing);
                post.Time = lastPost.Time?.Date.Add(nextTiming);
            }
            else
            {
                var timing = DateTime.Now.TimeOfDay;
                var nextTiming = settings.Timings.First(t => t > timing);
                post.Time = DateTime.Now.Date.Add(nextTiming);
            }
        }

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
        return database.Posts
            .AsNoTracking()
            .Include(p => p.Pictures)
            .ToList();
    }

    public void RemoveAll()
    {
        using var database = new DBService();
        database.Posts.RemoveRange(
            database.Posts.Include(p => p.Pictures));
        database.SaveChanges();
    }
}
