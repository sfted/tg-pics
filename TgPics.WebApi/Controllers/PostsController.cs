using Microsoft.AspNetCore.Mvc;
using TgPics.Core.Models;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Services;

namespace TgPics.WebApi.Controllers;

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
    public IActionResult Add(Post post)
    {
        service.Add(post);
        return Ok();
    }

    [Authorize]
    [HttpPost("remove")]
    public IActionResult Remove(Post post)
    {
        service.Remove(post);
        return Ok();
    }

    [Authorize]
    [HttpGet("getall")]
    public IActionResult GetAll()
    {
        return Ok(service.GetAll());
    }

    [Authorize]
    [HttpPost("removeall")]
    public IActionResult RemoveAll(string confirmation)
    {
        if (confirmation == "yeah, kill 'em all.")
        {
            service.RemoveAll();
            return Ok();
        }
        else return BadRequest(
            "Confirm deletion by passing 'confirmation=yeah, kill 'em all.'");
    }
}