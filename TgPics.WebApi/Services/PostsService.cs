namespace TgPics.WebApi.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgPics.Core.Models;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Models;

public interface IPostsService
{
    public void Add(AddPostRequest post);
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

    public void Add(AddPostRequest post)
    {
        using var database = new DBService();

        var postEntity = new Post
        {
            SourceLink = post.SourceLink,
            SourceTitle = post.SourceTitle,
            Comment = post.Comment,
            Pictures = post.Pictures.Select(p => new Picture
            {
                Url = p.Url,
                Position = p.Position
            })
        };

        foreach (var pic in postEntity.Pictures)
        {
            if (pic.Data == null && pic.Url != null)
                pic.Load();
        }

        if (postEntity.PublicationDateTime == null)
        {
            if (database.Posts.Any())
            {
                var lastPost = database.Posts
                    .OrderBy(p => p.PublicationDateTime).Last();

                var timing = lastPost.PublicationDateTime?.TimeOfDay;
                var nextTiming = settings.Timings.First(t => t > timing);
                
                postEntity.PublicationDateTime = lastPost
                    .PublicationDateTime?.Date.Add(nextTiming);
            }
            else
            {
                var timing = DateTime.Now.TimeOfDay;
                var nextTiming = settings.Timings.First(t => t > timing);
                postEntity.PublicationDateTime = DateTime.Now.Date.Add(nextTiming);
            }
        }

        database.Posts.Add(postEntity);
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
