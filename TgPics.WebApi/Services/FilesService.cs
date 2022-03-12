namespace TgPics.WebApi.Services;

using Microsoft.EntityFrameworkCore;
using TgPics.Core.Entities;
using TgPics.Core.Enums;
using TgPics.Core.Models;
using TgPics.Core.Models.Responses;

public interface IFilesService
{
    public string Get(int id);

    public FilesGetAllResponse GetAll(
        string host, int count, int offset);

    public Task<List<MediaFileInfo>> UploadAsync(
        string host, string root, IFormFile[] files);
}

public class FilesService : IFilesService
{
    public const string JPG = ".jpg";
    public const string JPEG = ".jpeg";
    public const string MP4 = ".mp4";
    public const string UPLOADS = "uploads";

    public string Get(int id)
    {
        using var database = new DBService();

        var file = database
            .Uploads
            .AsNoTracking()
            .First(f => f.Id == id);

        if (file == null)
            throw new FileNotFoundException($"File with id = '{id}' not found.");

        if (file.FileName == null)
            throw new Exception($"File with id = '{id}' has null filename.");

        return $"{UPLOADS}/{file.FileName}";
    }

    public FilesGetAllResponse GetAll(string host, int count, int offset)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(count), count, "Value must be positive.");

        if (offset < 0)
            throw new ArgumentOutOfRangeException(
                nameof(offset), offset, "Value must be at least zero.");

        using var database = new DBService();

        var files = database
            .Uploads
            .AsNoTracking()
            .Skip(offset)
            .Take(count)
            .Select(f => new MediaFileInfo(host, f))
            .ToList();

        return new FilesGetAllResponse(database.Uploads.Count(), files);
    }

    public async Task<List<MediaFileInfo>> UploadAsync(
        string host, string root, IFormFile[] files)
    {
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

            var info = await SaveFileAndGetInfo(host, root, file);
            infos.Add(info);
        }

        return infos;
    }

    static async Task<MediaFileInfo> SaveFileAndGetInfo(
        string host, string root, IFormFile file)
    {
        string uploads = Path.Combine(root, UPLOADS);

        if (!Directory.Exists(uploads))
            Directory.CreateDirectory(uploads);

        var media = new MediaFile();

        using var database = new DBService();
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