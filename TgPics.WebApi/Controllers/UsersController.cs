namespace TgPics.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Models;
using TgPics.WebApi.Services;

[ApiController]
[Route("api")]
public class UsersController : ControllerBase
{
    private readonly IUserService service;

    public UsersController(IUserService service)
    {
        this.service = service;
    }

    [HttpPost("login")]
    public IActionResult Authenticate(AuthenticateRequest model)
    {
        var response = service.Authenticate(model);

        if (response == null)
            return BadRequest(new { message = "Username or password is incorrect" });

        return Ok(response);
    }

    [Authorize]
    [HttpGet("users")]
    public IActionResult GetAll()
    {
        var users = service.GetAll();
        return Ok(users);
    }
}