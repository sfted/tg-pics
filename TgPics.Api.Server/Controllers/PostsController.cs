namespace TgPics.Api.Server.Controllers;

using Microsoft.AspNetCore.Mvc;
using TgPics.Core.Models.Requests;
using TgPics.Core.Models.Responses;
using TgPics.Api.Server.Helpers;
using TgPics.Api.Server.Services;

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
    [HttpGet("get")]
    public IActionResult Get(int id)
    {
        try
        {
            return Ok(service.Get(
                Request.GetFullHost(), id));
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponse(ex.Message));
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
            return BadRequest(new MessageResponse(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("add")]
    public IActionResult Add(PostsAddRequest request)
    {
        try
        {
            return Ok(service.Add(
                Request.GetFullHost(), request));
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponse(ex.Message));
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
        catch (ArgumentException ex)
        {
            return BadRequest(new MessageResponse(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("postnow")]
    public IActionResult PostNow(IdRequest request)
    {
        try
        {
            service.PostNow(request);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new MessageResponse(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("removeall")]
    public async Task<IActionResult> RemoveAllAsync(PostsRemoveAllRequest request)
    {
        try
        {
            var response = await service.RemoveAllAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new MessageResponse(ex.Message));
        }
    }
}