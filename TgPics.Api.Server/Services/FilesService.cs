namespace TgPics.Api.Server.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TgPics.Core.Entities;
using TgPics.Core.Enums;
using TgPics.Core.Models;
using TgPics.Core.Models.Requests;
using TgPics.Core.Models.Responses;

public interface IFilesService
{
    public IFileInfo Get(int id);

    public ListResponse<MediaFileInfo> GetAll(
        string host, int count, int offset);

    public Task<ListResponse<MediaFileInfo>> UploadAsync(
        string host, IFormFile[] files);

    public Task<int> UploadAsync(IFormFile file);

    public void Remove(IdRequest request);
    public void Remove(DatabaseService database, IdRequest request);
}

public class FilesService : IFilesService
{
    public FilesService(
        IWebHostEnvironment environment,
        DatabaseService database)
    {
        this.environment = environment;
        this.database = database;
    }

    private readonly IWebHostEnvironment environment;
    private readonly DatabaseService database;

    public const string JPG = ".jpg";
    public const string JPEG = ".jpeg";
    public const string MP4 = ".mp4";
    public const string UPLOADS = "uploads";

    public IFileInfo Get(int id)
    {
        var file = database
            .Uploads
            .AsNoTracking()
            .First(f => f.Id == id);

        if (file == null)
            throw new FileNotFoundException($"File with id = '{id}' not found.");

        if (file.FileName == null)
            throw new Exception($"File with id = '{id}' has null filename.");

        var info = environment
            .ContentRootFileProvider
            .GetFileInfo($"{UPLOADS}/{file.FileName}");

        return info;
    }

    public ListResponse<MediaFileInfo> GetAll(string host, int count, int offset)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(count), count, "Value must be positive.");

        if (offset < 0)
            throw new ArgumentOutOfRangeException(
                nameof(offset), offset, "Value must be at least zero.");

        var files = database
            .Uploads
            .AsNoTracking()
            .Skip(offset)
            .Take(count)
            .Select(f => new MediaFileInfo(host, f))
            .ToList();

        return new ListResponse<MediaFileInfo>(database.Uploads.Count(), files);
    }

    public async Task<ListResponse<MediaFileInfo>> UploadAsync(
        string host, IFormFile[] files)
    {
        if (!files.Any())
            throw new Exception("You must upload at least one file.");

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);

            if (extension != JPG && extension != JPEG && extension != MP4)
                throw new NotSupportedException($"This file format ({extension}) is not supported.");
        }

        var infos = new List<MediaFileInfo>();

        foreach (var file in files)
        {
            if (file.Length <= 0)
                continue;

            var info = await SaveFileAndGetInfo(
                host, environment.ContentRootPath, file);

            infos.Add(info);
        }

        return new ListResponse<MediaFileInfo>(infos.Count, infos);
    }

    public async Task<int> UploadAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName);

        if (extension != JPG && extension != JPEG && extension != MP4)
            throw new NotSupportedException($"This file format ({extension}) is not supported.");

        // небольшой костыль — host: null
        var info = await SaveFileAndGetInfo(
            null, environment.ContentRootPath, file);

        return info.Id;
    }

    public void Remove(IdRequest request)
    {
        Remove(database, request);
        database.SaveChanges();
    }

    public void Remove(DatabaseService database, IdRequest request)
    {
        var file = database
            .Uploads
            .First(f => f.Id == request.Id);

        var info = environment
            .ContentRootFileProvider
            .GetFileInfo($"{UPLOADS}/{file.FileName}");

        File.Delete(info.PhysicalPath);

        database.Remove(file);
    }

    async Task<MediaFileInfo> SaveFileAndGetInfo(
        string? host, string root, IFormFile file)
    {
        string uploads = Path.Combine(root, UPLOADS);

        if (!Directory.Exists(uploads))
            Directory.CreateDirectory(uploads);

        var media = new MediaFile();

        database.Uploads.Add(media);

        // так надо. (чтобы получить id)
        database.SaveChanges();

        switch (Path.GetExtension(file.FileName))
        {
            case JPG or JPEG:
                media.Type = MediaType.Picture;
                media.FileName = new Uri($"picture_{media.Id}{JPG}", UriKind.Relative);
                break;

            case MP4:
                media.Type = MediaType.Video;
                media.FileName = new Uri($"video_{media.Id}{MP4}", UriKind.Relative);
                break;
        }

        database.SaveChanges();

        if (media.FileName == null)
            throw new Exception("media.FileName is null (idk how).");

        using var stream = new FileStream(
            Path.Combine(uploads, media.FileName.ToString()),
            FileMode.Create);

        await file.CopyToAsync(stream);

        return new MediaFileInfo(host, media);
    }
}