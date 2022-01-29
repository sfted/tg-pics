// https://github.com/cornflourblue/dotnet-6-jwt-authentication-api

using Microsoft.Extensions.Options;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

services.AddCors();
services.AddControllers();

services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

services.AddScoped<IUserService, UserService>();
services.AddScoped<IPostsService, PostsService>();

services.AddHostedService<BotHostedService>(s =>
    new BotHostedService(
        builder.Configuration["BotToken"],
        builder.Configuration["TgAdminChatId"],
        builder.Configuration["TgChannelUsername"]));

var app = builder.Build();

app.Services
    .GetRequiredService<IOptions<AppSettings>>()
    .Value.WebApiKey = builder.Configuration["WebApiKey"];

UserService.Users[0].Password =
    builder.Configuration["AdminPwd"];

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();
app.UseStaticFiles();

app.Run();