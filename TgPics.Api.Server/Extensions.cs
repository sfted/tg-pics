using Microsoft.EntityFrameworkCore;
using TgPics.Api.Server.Services;

namespace TgPics.Api.Server;

public static class Extensions
{
    public static IServiceCollection AddSettings(
        this IServiceCollection services, ConfigurationManager configuration)
    {
        services.AddSingleton<ISettingsService, SettingsService>(provider =>
            new SettingsService(
                Convert.ToInt32(configuration["PostsPerDay"]),
                configuration["WebApiKey"],
                configuration["AdminPwd"],
                configuration["BotToken"],
                configuration["TgAdminChatId"],
                configuration["TgChannelUsername"],
                configuration["VkApiToken"],
                Convert.ToInt32(configuration["VkGroupId"]),
                Convert.ToInt32(configuration["VkAppId"]),
                Convert.ToInt32(configuration["VkUserId"]),
                configuration["VkVideoTitle"]
            )
        );

        return services;
    }

    public static IServiceCollection AddDatabase(
        this IServiceCollection services, string connection)
    {
        services.AddTransient(provider =>
            {
                var options = new DbContextOptionsBuilder<DatabaseService>()
                    .UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 11)));

                return new DatabaseService(options.Options);
            }
        );

        return services;
    }
}