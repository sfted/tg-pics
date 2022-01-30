namespace TgPics.WebApi.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TgPics.Core.Entities;
using TgPics.Core.Models;
using TgPics.WebApi.Helpers;

public interface IPostsService
{
    public void Add(AddPostRequest request);
    public void Remove(RemovePostRequest request);
    public GetAllPostsResponse GetAll();
    public void RemoveAll(RemoveAllPostsRequest request);
}

public class PostsService : IPostsService
{
    public PostsService(IOptions<AppSettings> settings)
    {
        this.settings = settings.Value;
    }

    private const string CONFIRMATION = "yeah, kill 'em all.";

    private readonly AppSettings settings;

    public void Add(AddPostRequest request)
    {
        using var database = new DBService();

        var post = new Post
        {
            SourceLink = request.SourceLink,
            SourceTitle = request.SourceTitle,
            Comment = request.Comment,
            Pictures = request.Pictures.Select(p => new Picture
            {
                Url = p.Url,
                Position = p.Position
            })
        };

        foreach (var pic in post.Pictures)
        {
            if (pic.Data == null && pic.Url != null)
                pic.Load();
        }

        if (post.PublicationDateTime == null)
        {
            if (database.Posts.Any())
            {
                var lastPost = database.Posts
                    .OrderBy(p => p.PublicationDateTime).Last();

                var timing = lastPost.PublicationDateTime?.TimeOfDay;
                var nextTiming = settings.Timings.First(t => t > timing);

                post.PublicationDateTime = lastPost
                    .PublicationDateTime?.Date.Add(nextTiming);
            }
            else
            {
                var timing = DateTime.Now.TimeOfDay;
                var nextTiming = settings.Timings.First(t => t > timing);
                post.PublicationDateTime = DateTime.Now.Date.Add(nextTiming);
            }
        }

        database.Posts.Add(post);
        database.SaveChanges();
    }

    public void Remove(RemovePostRequest request)
    {
        using var database = new DBService();

        var post = database.Posts
            .Include(p => p.Pictures)
            .FirstOrDefault(p => p.Id == request.PostId);

        if (post != null)
        {
            database.Posts.Remove(post);
            database.SaveChanges();
        }
        else throw new ArgumentException(
            "A post with this id was not found.", nameof(request));
    }

    public GetAllPostsResponse GetAll()
    {
        using var database = new DBService();
        return new GetAllPostsResponse
        {
            Posts = database.Posts
                .AsNoTracking()
                .Include(p => p.Pictures)
                .ToList()
        };
    }

    public void RemoveAll(RemoveAllPostsRequest request)
    {
        if (request.Confirmation == CONFIRMATION)
        {
            using var database = new DBService();
            database.Posts.RemoveRange(
                database.Posts.Include(p => p.Pictures));
            database.SaveChanges();
        }
        else throw new ArgumentException(
            $"To confirm the removal, you should pass Confirmation='{CONFIRMATION}'",
            nameof(request));
    }
}
