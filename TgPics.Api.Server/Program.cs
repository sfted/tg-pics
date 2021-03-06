using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System.Net;
using TgPics.Api.Server;
using TgPics.Api.Server.Helpers;
using TgPics.Api.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// services
var connection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services
    .Configure<FormOptions>(options =>
    {
        // https://stackoverflow.com/questions/40364226
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartBodyLengthLimit = int.MaxValue;
    })
    .AddSettings(builder.Configuration)
    .AddDatabase(connection)
    .AddTransient<IUsersService, UsersService>()
    .AddTransient<IPostsService, PostsService>()
    .AddTransient<IFilesService, FilesService>()
    .AddHostedService<BotService>()
    // ???
    // TODO: ??????????? ?? ??? ?? ?????
    .AddCors()
    .AddControllers();


var app = builder.Build();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// https://github.com/cornflourblue/dotnet-6-jwt-authentication-api
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.Run();