namespace TgPics.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Models;
using TgPics.WebApi.Services;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    public PostsController(IPostsService service)
    {
        this.service = service;
    }

    private readonly IPostsService service;

    [Authorize]
    [HttpPost("add")]
    public IActionResult Add(AddPostRequest request)
    {
        service.Add(request);
        return Ok();
    }

    [Authorize]
    [HttpPost("remove")]
    public IActionResult Remove(RemovePostRequest request)
    {
        try
        {
            service.Remove(request);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("getall")]
    public IActionResult GetAll() => Ok(service.GetAll());

    [Authorize]
    [HttpPost("removeall")]
    public IActionResult RemoveAll(RemoveAllPostsRequest request)
    {
        try
        {
            service.RemoveAll(request);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}