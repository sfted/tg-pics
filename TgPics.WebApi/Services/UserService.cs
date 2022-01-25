﻿namespace TgPics.WebApi.Services;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TgPics.WebApi.Entities;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Models;

public interface IUserService
{
    AuthenticateResponse Authenticate(AuthenticateRequest model);
    IEnumerable<User> GetAll();
    User GetById(int id);
}

public class UserService : IUserService
{
    // users hardcoded for simplicity, store in a db with hashed passwords in production applications
    public readonly static List<User> Users = new()
    {
        new User { Id = 1, Username = "admin" }
    };

    private readonly AppSettings settings;

    public UserService(IOptions<AppSettings> settings)
    {
        this.settings = settings.Value;
    }

    public AuthenticateResponse Authenticate(AuthenticateRequest model)
    {
        var user = Users.SingleOrDefault(x => x.Username == model.Username && x.Password == model.Password);

        // return null if user not found
        if (user == null) return null;

        // authentication successful so generate jwt token
        var token = GenerateJwtToken(user);

        return new AuthenticateResponse(user, token);
    }

    public IEnumerable<User> GetAll()
    {
        return Users;
    }

    public User GetById(int id)
    {
        return Users.FirstOrDefault(x => x.Id == id);
    }

    // helper methods

    private string GenerateJwtToken(User user)
    {
        // generate token that is valid for 7 days
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(settings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}