namespace TgPics.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Services;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    public FilesController(
        IFilesService service,
        IWebHostEnvironment environment)
    {
        this.service = service;
        this.environment = environment;
    }

    readonly IFilesService service;
    readonly IWebHostEnvironment environment;

    [HttpGet("get")]
    public IActionResult Get(int id)
    {
        try
        {
            var filename = service.Get(id);
            var extension = Path.GetExtension(filename);

            var file = environment
                .ContentRootFileProvider
                .GetFileInfo(filename)
                .CreateReadStream();

            return extension switch
            {
                FilesService.JPG or FilesService.JPEG => File(file, "image/jpeg"),
                FilesService.MP4 => File(file, "video/mp4", enableRangeProcessing: true),
                _ => throw new NotSupportedException(
                    $"This file type ('{extension}') is not supported.")
            };
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("getall")]
    public IActionResult GetAll(int count = 10, int offset = 0)
    {
        try
        {
            var response = service.GetAll(
                Request.GetFullHost(), count, offset);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("upload")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Upload([FromForm] IFormFile[] files)
    {
        try
        {
            var infos = await service.UploadAsync(
                Request.GetFullHost(),
                environment.ContentRootPath,
                files);

            return Ok(infos);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}