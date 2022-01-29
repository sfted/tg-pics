using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgPics.Core.Models;

namespace TgPics.WebApi.Services;

public class BotHostedService : IHostedService, IDisposable
{
    public BotHostedService(
        string botToken,
        string adminChatId,
        string channelUsername)
    {
        this.adminChatId = adminChatId;
        this.channelUsername = channelUsername;

        InitializeTelegramBot(botToken);
    }

    private Timer timer;
    private TelegramBotClient bot;

    private readonly string adminChatId;
    private readonly string channelUsername;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => timer?.Dispose();

    private async void InitializeTelegramBot(string botToken)
    {
        bot = new TelegramBotClient(botToken);
        var channelTitle = bot.GetChatAsync(channelUsername).Result.Title;
        var url = channelUsername.Replace("@", "https://t.me/");

        try
        {
            await bot.SendTextMessageAsync(adminChatId,
                $"ℹ️ Бот запущен.\nКанал: [{channelTitle}]({url})",
                ParseMode.Markdown,
                disableWebPagePreview: false);
        }
        catch { }
    }

    private void DoWork(object state) =>
        CheckForNewPostsAndPostIfThereAreAny();

    private async void CheckForNewPostsAndPostIfThereAreAny()
    {
        using var database = new DBService();
        if (database.Posts.Any(p => p.Time < DateTime.Now))
        {
            var post = database.Posts
                .Include(p => p.Pictures)
                .Where(p => p.Time < DateTime.Now)
                .First();

            if (post != null && post.Pictures.Any())
            {
                await bot.SendMediaGroupAsync(
                    channelUsername, MakeInputMedia(post));

                database.Posts.Remove(post);
                database.SaveChanges();
            }
        }
    }

    private static List<InputMediaPhoto> MakeInputMedia(Post post)
    {
        var pictures = post.Pictures
                        .OrderBy(p => p.Position)
                        .ToList();

        var inputMediaPics = new List<InputMediaPhoto>();
        for (int i = 0; i < pictures.Count; i++)
        {
            var media = new InputMedia(
                    new MemoryStream(pictures[i].Data),
                    pictures[i].Id.ToString());

            if (i == 0)
            {
                var caption = $"src: [{post.SourceTitle}]({post.SourceLink})";

                if (!string.IsNullOrEmpty(post.Text))
                    caption.Insert(0, "text\n");

                inputMediaPics.Add(
                    new InputMediaPhoto(media)
                    {
                        ParseMode = ParseMode.Markdown,
                        Caption = caption
                    });
            }
            else
                inputMediaPics.Add(new InputMediaPhoto(media));
        }

        return inputMediaPics;
    }
}