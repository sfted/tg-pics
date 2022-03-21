using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TgPics.WebApi.Services;

namespace TgPics.WebApi.Helpers;

public class JwtMiddleware
{
    public JwtMiddleware(
        RequestDelegate next,
        ISettingsService settingsService)
    {
        this.next = next;
        this.settingsService = settingsService;
    }

    public const string USER_KEY = "User";

    readonly RequestDelegate next;
    readonly ISettingsService settingsService;

    public async Task Invoke(HttpContext context, IUserService service)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (token != null)
            AttachUserToContext(context, service, token);

        await next(context);
    }

    void AttachUserToContext(
        HttpContext context, IUserService userService, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settingsService.WebApiKey);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                // set clockskew to zero so tokens expire exactly at token expiration time
                // (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

            // attach user to context on successful jwt validation
            context.Items[USER_KEY] = userService.GetById(userId);
        }
        catch
        {
            // do nothing if jwt validation fails
            // user is not attached to context so request won't have access to secure routes
        }
    }
}