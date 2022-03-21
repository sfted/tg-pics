using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgPics.Core.Entities;
using TgPics.Core.Models.Requests;

namespace TgPics.WebApi.Services;

public class BotService : IHostedService, IDisposable
{
    public BotService(
        ISettingsService settingsService,
        IPostService postService,
        IFileService fileService,
        DatabaseService database)
    {
        tgAdminChatId = settingsService.TgAdminChatId;
        tgChannelUsername = settingsService.TgChannelUsername;

        this.postService = postService;
        this.fileService = fileService;
        this.database = database;

        bot = InitializeTelegramBot(settingsService.BotToken);
        SendApiStartedNotification();
    }

    private Timer? timer;
    private readonly TelegramBotClient bot;

    private readonly string tgAdminChatId;
    private readonly string tgChannelUsername;
    //private readonly string vkPublicUsername;
    private readonly IPostService postService;
    private readonly IFileService fileService;
    private readonly DatabaseService database;

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

    public void Dispose()
    {
        timer?.Dispose();
    }

    TelegramBotClient InitializeTelegramBot(string botToken)
    {
        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };

        var bot = new TelegramBotClient(botToken);

        bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token);

        return bot;
    }

    async void SendApiStartedNotification()
    {
        var channelTitle = bot.GetChatAsync(tgChannelUsername).Result.Title;
        var url = tgChannelUsername.Replace("@", "https://t.me/");

        try
        {
            await bot.SendTextMessageAsync(tgAdminChatId,
                $"ℹ️ Бот запущен.\nКанал: [{channelTitle}]({url})",
                ParseMode.Markdown,
                disableWebPagePreview: false);
        }
        catch { }
    }

    async void DoWork(object? state) =>
        await CheckForNewPostsAndPostIfThereAreAny();

    async Task CheckForNewPostsAndPostIfThereAreAny()
    {
        // TODO: перенести это в сервис постов
        if (database.Posts.Any(p => p.PublicationDateTime < DateTime.Now))
        {
            var post = database.Posts
                .Include(p => p.Media)
                .Where(p => p.PublicationDateTime < DateTime.Now &&
                    p.ManagedToPublish != false)
                .FirstOrDefault();

            if (post != null && post.Media.Any())
            {
                try
                {
                    await PostToTelegram(tgChannelUsername, post);

                    postService.Remove(new IdRequest(post.Id));
                }
                catch (Exception ex)
                {
                    post.ManagedToPublish = false;
                    post.PublicationError = ex.ToString();
                    database.SaveChanges();

                    try
                    {
                        await bot.SendTextMessageAsync(tgAdminChatId,
                            $"❌ Ошибка.\nНе удалось опубликоват пост с id=`{post.Id}`" +
                            $" [{post.SourceTitle}]({post.SourceLink})\nТекст ошибки: `{post.PublicationError}`",
                            ParseMode.Markdown,
                            disableWebPagePreview: false);
                    }
                    catch { }
                }
            }
        }
    }

    async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            //var query = update.CallbackQuery;
            ////if (query == null) return;
            //
            //var message = query.Message;
            ////if (message == null) return;
            //
            //var data = query.Data;
            ////if (data == null) return;
            //
            //var chatId = message.Chat.Id;
            ////if (chatId.ToString() != adminChatId) return;
            //
            //try
            //{
            //    var items = data.Split(' ');
            //    Console.WriteLine(data);
            //
            //    if (items[0] == "postnow")
            //    {
            //        var id = Convert.ToInt32(items[1]);
            //        postService.PostNow(new IdRequest(id));
            //
            //        await botClient.EditMessageReplyMarkupAsync(
            //            chatId,
            //            message.MessageId,
            //            new InlineKeyboardMarkup(new InlineKeyboardButton("✅")),
            //            cancellationToken);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    await botClient.SendTextMessageAsync(
            //        chatId,
            //        $"❌ Что-то пошло не так: '{ex}'.",
            //        ParseMode.Markdown,
            //        cancellationToken: cancellationToken);
            //}
        }
        else if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            if (message == null) return;

            var chatId = message.Chat.Id;
            if (chatId.ToString() != tgAdminChatId) return;

            if (message.Type == MessageType.Text &&
                message.Text != null)
            {
                //var text = message.Text;
                //if (text == "/postnow")
                //{
                //    using var database = new DatabaseService();
                //
                //    var ids = database
                //        .Posts
                //        .AsNoTracking()
                //        .Select(p => p.Id)
                //        .ToList();
                //
                //    var buttons = new List<InlineKeyboardButton>();
                //    foreach (var id in ids)
                //    {
                //        buttons.Add(new InlineKeyboardButton(id.ToString())
                //        {
                //            CallbackData = $"postnow {id}"
                //        });
                //    }
                //
                //    await botClient.SendTextMessageAsync(
                //        chatId,
                //        "❔ Выбери пост",
                //        replyMarkup: new InlineKeyboardMarkup(buttons),
                //        cancellationToken: cancellationToken);
                //}
            }

            else if (message.Type == MessageType.Photo &&
                message.Photo != null &&
                message.ForwardFromChat != null &&
                !string.IsNullOrEmpty(message.ForwardFromChat.Username))
            {
                using var stream = new MemoryStream();

                await botClient.GetInfoAndDownloadFileAsync(
                    message.Photo.Last().FileId, stream, cancellationToken);

                var mediaId = await fileService.UploadAsync(
                    new FormFile(stream, 0, stream.Length, "file", "file.jpg"));

                if (message.ForwardFromChat.Username == null)
                {
                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"❌ Посты с закрытых каналов пока не поддерживаются.",
                        cancellationToken: cancellationToken); ;
                }
                else
                {
                    var post = postService.Add(
                        new PostsAddRequest(
                            new List<int>() { mediaId },
                            $"https://t.me/{message.ForwardFromChat.Username}/{message.ForwardFromMessageId}",
                            "tg",
                            message.ForwardFromChat.Title ??
                                message.ForwardFromChat.Username
                        ));


                    await botClient.SendTextMessageAsync(
                        chatId,
                        $"✅ Пост добавлен в очередь. `id = {post.Id}`," +
                            $" дата и время публикации: `{post.PublicationDateTime}`",
                        ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                }
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    $"❌ Данное сообщение не поддерживается.",
                    cancellationToken: cancellationToken);
            }
        }
    }

    Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var message = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram BOT Error:\n" +
                    $"[{apiRequestException.ErrorCode}]\n" +
                    $"{apiRequestException.Message}",

            _ => exception.ToString()
        };

        Console.WriteLine(message);

        return Task.CompletedTask;
    }

    async Task PostToTelegram(string channelUsername, Post post)
    {
        await bot.SendMediaGroupAsync(
            channelUsername, MakeInputMedia(post));
    }

    async Task PostToVkontakte(string publicUsername, Post post)
    {

    }

    List<IAlbumInputMedia> MakeInputMedia(Post post)
    {
        var medias = post.Media.ToList();

        var result = new List<IAlbumInputMedia>();
        for (int i = 0; i < medias.Count; i++)
        {
            var info = fileService.Get(medias[i].Id);
            var extension = Path.GetExtension(info.Name);
            var file = info.CreateReadStream();

            // TODO: сделай проверку на null
            var media = new InputMedia(file, medias[i].Id.ToString());

            if (i == 0)
            {
                var caption = $"src: [{post.SourcePlatform} //" +
                    $" {post.SourceTitle}]({post.SourceLink})";

                if (!string.IsNullOrEmpty(post.InviteLink))
                    caption += $" [// inv ✉️]({post.InviteLink})";

                if (!string.IsNullOrEmpty(post.Comment))
                    caption = caption.Insert(0, $"{post.Comment}\n");

                IAlbumInputMedia? finalMedia = null;
                if (extension is FileService.JPEG or FileService.JPG)
                {
                    finalMedia = new InputMediaPhoto(media)
                    {
                        ParseMode = ParseMode.Markdown,
                        Caption = caption
                    };
                }
                else if (extension is FileService.MP4)
                {
                    finalMedia = new InputMediaVideo(media)
                    {
                        ParseMode = ParseMode.Markdown,
                        Caption = caption,
                        SupportsStreaming = true
                    };
                }

                if (finalMedia != null)
                    result.Add(finalMedia);
            }
            else
            {
                IAlbumInputMedia? finalMedia = null;

                if (extension is FileService.JPEG or FileService.JPG)
                    finalMedia = new InputMediaPhoto(media);
                else if (extension is FileService.MP4)
                    finalMedia = new InputMediaVideo(media) { SupportsStreaming = true };

                if (finalMedia != null)
                    result.Add(finalMedia);
            }
        }

        return result;
    }
}