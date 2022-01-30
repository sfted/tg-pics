namespace TgPics.Core.Models;

using TgPics.Core.Entities;

public class AuthenticateResponse
{
    public AuthenticateResponse() { }

    public AuthenticateResponse(User user, string token)
    {
        Id = user.Id;
        Username = user.Username;
        Token = token;
    }

    public int Id { get; set; }
    public string Username { get; set; }
    public string Token { get; set; }
}