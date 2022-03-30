namespace TgPics.WebApi.Services;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TgPics.Core.Entities;
using TgPics.Core.Models.Requests;
using TgPics.Core.Models.Responses;

public interface IUsersService
{
    UsersAuthResponse? Authenticate(UsersAuthRequest model);
    List<User> GetAll();
    User? GetById(int id);
}

public class UsersService : IUsersService
{
    // >users hardcoded for simplicity, store in a db
    // >with hashed passwords in production applications
    // дадададада
    public readonly List<User> Users = new()
    {
        new User { Id = 1, Username = "admin" }
    };

    private readonly ISettingsService settings;

    public UsersService(ISettingsService settings)
    {
        this.settings = settings;

        Users[0].Password = settings.AdminPwd;
    }

    public UsersAuthResponse? Authenticate(UsersAuthRequest model)
    {
        var user = Users.SingleOrDefault(
            u => u.Username == model.Username && u.Password == model.Password);

        if (user == null) return null;

        return new UsersAuthResponse(user, GenerateJwtToken(user));
    }

    public List<User> GetAll() => Users;

    public User? GetById(int id) =>
        Users.FirstOrDefault(x => x.Id == id);

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(settings.WebApiKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}