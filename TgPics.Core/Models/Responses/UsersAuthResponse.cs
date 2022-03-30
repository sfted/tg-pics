namespace TgPics.Core.Models.Responses;

using TgPics.Core.Entities;

public class UsersAuthResponse
{
    public UsersAuthResponse() { }

    public UsersAuthResponse(User user, string token)
    {
        Id = user.Id;
        Username = user.Username;
        Token = token;
    }

    public int Id { get; set; }
    public string Username { get; set; }
    public string Token { get; set; }
}