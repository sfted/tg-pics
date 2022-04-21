namespace TgPics.Api.Server.Controllers;

using Microsoft.AspNetCore.Mvc;
using TgPics.Core.Models.Requests;
using TgPics.Core.Models.Responses;
using TgPics.Api.Server.Services;

[ApiController]
[Route("api")]
public class UsersController : ControllerBase
{
    private readonly IUsersService service;

    public UsersController(IUsersService service)
    {
        this.service = service;
    }

    [HttpPost("auth")]
    public IActionResult Authenticate(UsersAuthRequest request)
    {
        var response = service.Authenticate(request);

        if (response == null)
            return BadRequest(new MessageResponse("Неверный юзернейм или пароль"));

        return Ok(response);
    }
}