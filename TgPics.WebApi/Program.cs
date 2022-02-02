// https://github.com/cornflourblue/dotnet-6-jwt-authentication-api

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
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

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
//services.AddDbContextPool<DBService>(
//      options => options.UseMySql(connection, ServerVersion.AutoDetect(connection))
//   );
DBService.ConnectionString = connection;

var app = builder.Build();

var settings = app.Services
    .GetRequiredService<IOptions<AppSettings>>()
    .Value;

settings.WebApiKey = builder.Configuration["WebApiKey"];
settings.PostPerDay =
    Convert.ToInt32(builder.Configuration["PostsPerDay"]);

UserService.Users[0].Password =
    builder.Configuration["AdminPwd"];

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.Run();