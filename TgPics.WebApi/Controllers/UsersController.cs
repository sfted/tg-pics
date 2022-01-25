namespace TgPics.WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Models;
using TgPics.WebApi.Services;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService service;

    public UsersController(IUserService service)
    {
        this.service = service;
    }

    [HttpPost("authenticate")]
    public IActionResult Authenticate(AuthenticateRequest model)
    {
        var response = service.Authenticate(model);

        if (response == null)
            return BadRequest(new { message = "Username or password is incorrect" });

        return Ok(response);
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetAll()
    {
        var users = service.GetAll();
        return Ok(users);
    }
}