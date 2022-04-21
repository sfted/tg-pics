namespace TgPics.Api.Server.Services;

using Microsoft.EntityFrameworkCore;
using TgPics.Core.Entities;
using TgPics.Core.Models;
using TgPics.Core.Models.Requests;
using TgPics.Core.Models.Responses;

public interface IPostsService
{
    public PostModel Get(string host, int id);
    public PostsGetAllResponse GetAll(string host, int count = 10, int offset = 0);
    public PostModel Add(string host, PostsAddRequest request);
    public Post Add(PostsAddRequest request);
    public void Remove(IdRequest request);
    public void PostNow(IdRequest request);
    public Task<MessageResponse> RemoveAllAsync(PostsRemoveAllRequest request);
}

public class PostsService : IPostsService
{
    public PostsService(
        ISettingsService settingsService,
        IFilesService fileService,
        DatabaseService database)
    {
        this.settingsService = settingsService;
        this.fileService = fileService;
        this.database = database;
    }

    private const string CONFIRMATION = "yeah, kill 'em all.";

    private readonly ISettingsService settingsService;
    private readonly IFilesService fileService;
    private readonly DatabaseService database;

    public PostModel Get(string host, int id)
    {
        var post = database.Posts
            .AsNoTracking()
            .FirstOrDefault(p => p.Id == id);

        if (post == null)
            throw new Exception($"Постов id='{id}' не найдено.");

        return new PostModel(host, post);
    }

    public PostsGetAllResponse GetAll(string host, int count, int offset)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(count), count, "Значение должно быть больше нуля.");

        if (offset < 0)
            throw new ArgumentOutOfRangeException(
                nameof(offset), offset, "Значение должно быть больше либо равно нулю.");

        var posts = database.Posts
            .AsNoTracking()
            .Skip(offset)
            .Take(count)
            .Include(p => p.Media)
            .Select(p => new PostModel(host, p))
            .ToList();

        return new PostsGetAllResponse(database.Posts.Count(), posts);
    }

    public PostModel Add(string host, PostsAddRequest request)
    {
        return new PostModel(host, Add(request));
    }

    public Post Add(PostsAddRequest request)
    {
        var post = new Post
        {
            SourceLink = request.SourceLink,
            SourcePlatform = request.SourcePlatfrom,
            SourceTitle = request.SourceTitle,
            Comment = request.Comment
        };

        foreach (var mediaId in request.MediaIds)
        {
            var media = database.Uploads.First(f => f.Id == mediaId);

            if (media == null)
                throw new Exception($"Не удалось добавить пост в очередь." +
                    $" Медиафайлов с id='{mediaId}' не найдено.");

            post.Media.Add(media);
        }

        if (database.Posts.Any())
        {
            var lastPost = database.Posts
                .OrderBy(p => p.PublicationDateTime).Last();

            var timing = lastPost.PublicationDateTime.TimeOfDay;
            var nextTiming = settingsService.Timings.First(t => t > timing);

            post.PublicationDateTime = lastPost
                .PublicationDateTime.Date.Add(nextTiming);
        }
        else
        {
            var timing = DateTime.Now.TimeOfDay;
            var nextTiming = settingsService.Timings.First(t => t > timing);
            post.PublicationDateTime = DateTime.Now.Date.Add(nextTiming);
        }

        var entry = database.Posts.Add(post);
        database.SaveChanges();

        if (entry != null)
            return entry.Entity;
        else
            throw new Exception("Не удалось добавить пост в очередь.");
    }

    public void Remove(IdRequest request)
    {
        var post = database.Posts
            .Include(p => p.Media)
            .FirstOrDefault(p => p.Id == request.Id);

        if (post != null)
        {
            post.Media.ForEach(m =>
                fileService.Remove(database, new IdRequest(m.Id)));

            database.Posts.Remove(post);
            database.SaveChanges();
        }
        else throw new ArgumentException(
            $"Поста с таким id='{request.Id}' не существует.", nameof(request));
    }

    public void PostNow(IdRequest request)
    {
        var post = database.Posts
            .FirstOrDefault(p => p.Id == request.Id);

        if (post != null)
        {
            post.PublicationDateTime = DateTime.Now;
            database.SaveChanges();
        }
        else throw new ArgumentException(
            $"Поста с таким id='{request.Id}' не существует.", nameof(request));
    }


    public async Task<MessageResponse> RemoveAllAsync(PostsRemoveAllRequest request)
    {
        if (request.Confirmation == CONFIRMATION)
        {
            database.Posts.RemoveRange(database.Posts);

            await database.Uploads.ForEachAsync(
                f => fileService.Remove(database, new IdRequest(f.Id)));

            int deleted = database.SaveChanges();

            return new MessageResponse($"Из БД было удалено '{deleted}' сущностей.");
        }
        else throw new ArgumentException(
            $"Чтобы подтвердить удаление необходимо передать" +
            $" подтверждение: {{ confirmation='{CONFIRMATION}' }}",
            nameof(request));
    }
}
