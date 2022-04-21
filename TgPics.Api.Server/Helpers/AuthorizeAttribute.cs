namespace TgPics.Api.Server.Helpers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TgPics.Core.Entities;
using TgPics.Core.Models.Responses;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Items["User"] is not User)
        {
            var result = new JsonResult(new MessageResponse("Unauthorized"))
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };

            context.Result = result;
        }
    }
}