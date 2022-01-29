// https://github.com/cornflourblue/dotnet-6-jwt-authentication-api

using Microsoft.Extensions.Options;
using TgPics.WebApi.Helpers;
using TgPics.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// add services to DI container
{
    var services = builder.Services;
    services.AddCors();
    services.AddControllers();

    // configure strongly typed settings object
    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    // configure DI for application services
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IPostsService, PostsService>();
}

var app = builder.Build();

var settings = app.Services
    .GetRequiredService<IOptions<AppSettings>>().Value;

settings.WebApiKey = builder.Configuration["WebApiKey"];
settings.BotToken = builder.Configuration["BotToken"];

UserService.Users[0].Password =
    builder.Configuration["AdminPwd"];

// configure HTTP request pipeline
{
    // global cors policy
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());

    // custom jwt auth middleware
    app.UseMiddleware<JwtMiddleware>();

    app.MapControllers();
    app.UseStaticFiles();
}

app.Run();