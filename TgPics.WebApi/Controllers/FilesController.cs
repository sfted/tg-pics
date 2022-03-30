namespace TgPics.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TgPics.Core.Models.Requests;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Services;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    public FilesController(IFilesService service)
    {
        this.service = service;
    }

    readonly IFilesService service;

    [HttpGet("get")]
    public IActionResult Get(int id)
    {
        try
        {
            var info = service.Get(id);
            var extension = Path.GetExtension(info.Name);
            var file = info.CreateReadStream();

            return extension switch
            {
                FilesService.JPG or FilesService.JPEG => File(file, "image/jpeg"),
                FilesService.MP4 => File(file, "video/mp4", enableRangeProcessing: true),
                _ => throw new NotSupportedException(
                    $"Этот тип файла ('{extension}') не поддерживается.")
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
            return Ok(service.GetAll(
                Request.GetFullHost(), count, offset));
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
                Request.GetFullHost(), files);

            return Ok(infos);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost("remove")]
    public IActionResult Remove(IdRequest request)
    {
        try
        {
            service.Remove(request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}