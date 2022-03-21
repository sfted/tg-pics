// https://github.com/cornflourblue/dotnet-6-jwt-authentication-api

using Microsoft.AspNetCore.Http.Features;
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

{
    services.AddSingleton<IUserService, UserService>();
    services.AddSingleton<IPostService, PostService>();
    services.AddSingleton<IFileService, FileService>();

    services.AddHostedService<BotService>(provider =>
        new BotService(
            builder.Configuration["BotToken"],
            builder.Configuration["TgAdminChatId"],
            builder.Configuration["TgChannelUsername"],
            provider.GetRequiredService<IPostService>(),
            provider.GetRequiredService<IFileService>()));
}

//https://stackoverflow.com/questions/40364226
services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
});

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
//services.AddDbContextPool<DBService>(
//      options => options.UseMySql(connection, ServerVersion.AutoDetect(connection))
//   );
DatabaseService.ConnectionString = connection;

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
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.Run();