using Microsoft.EntityFrameworkCore;
using RestSharp;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TgPics.Core.Enums;
using TgPics.Core.Models.Requests;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace TgPics.WebApi.Services;

// TODO: ПЕРЕПИСАТЬ И ОТРЕФАКТОРИТЬ ВСЁ НАХУЙ

public class BotService : IHostedService, IDisposable
{
    public BotService(
        ISettingsService settingsService,
        IPostsService postService,
        IFilesService fileService,
        DatabaseService database)
    {
        this.settingsService = settingsService;
        this.postService = postService;
        this.fileService = fileService;
        this.database = database;

        bot = InitializeTelegramBot(settingsService.BotToken);
        SendApiStartedNotification();
    }

    private Timer? timer;
    private readonly TelegramBotClient bot;

    private readonly ISettingsService settingsService;
    private readonly IPostsService postService;
    private readonly IFilesService fileService;
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
        var channelTitle = bot.GetChatAsync(settingsService.TgChannelUsername).Result.Title;
        var url = settingsService.TgChannelUsername.Replace("@", "https://t.me/");

        try
        {
            await bot.SendTextMessageAsync(settingsService.TgAdminChatId,
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
                    await PostToTelegram(post);
                    await PostToVkontakte(post);

                    postService.Remove(new IdRequest(post.Id));
                }
                catch (Exception ex)
                {
                    post.ManagedToPublish = false;
                    post.PublicationError = ex.ToString();
                    database.SaveChanges();

                    await bot.SendTextMessageAsync(settingsService.TgAdminChatId,
                        $"❌ Ошибка.\nНе удалось опубликоват пост с id=`{post.Id}`" +
                        $" [{post.SourceTitle}]({post.SourceLink})\nТекст ошибки: `{post.PublicationError}`",
                        ParseMode.Markdown,
                        disableWebPagePreview: false);
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
            if (chatId.ToString() != settingsService.TgAdminChatId) return;

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

    async Task PostToTelegram(Core.Entities.Post post)
    {
        await bot.SendMediaGroupAsync(
            settingsService.TgChannelUsername,
            MakeTelegramInputMedia(post));
    }

    // TODO: переписать, разбить на методы
    //       и мб даже вынести в отдельный класс.
    async Task PostToVkontakte(Core.Entities.Post post)
    {
        var groupId = settingsService.VkGroupId;

        var api = new VkApi();
        api.Authorize(new ApiAuthParams
        {
            AccessToken = settingsService.VkApiToken,
            ApplicationId = Convert.ToUInt64(settingsService.VkAppId),
            UserId = Convert.ToInt64(settingsService.VkUserId),
            Settings = Settings.All
        });

        var albums = api.Photo.GetAlbums(new PhotoGetAlbumsParams
        {
            OwnerId = -groupId
        });

        var title = $"{DateTime.Now.Year}Q{GetQuarter(DateTime.Now)}";
        var album = albums.Where(a => a.Title == title).FirstOrDefault();

        if (album == null)
        {
            album = api.Photo.CreateAlbum(new PhotoCreateAlbumParams
            {
                Title = title,
                GroupId = groupId,
                UploadByAdminsOnly = true,
                CommentsDisabled = true,
            });
        }

        var caption = $"src: {post.SourcePlatform} // {post.SourceTitle}\n" +
            $"url: {post.SourceLink}";

        if (!string.IsNullOrEmpty(post.InviteLink))
            caption += $"\ninv: {post.InviteLink}";

        // TODO: сделать ограничение на кол-во фоток/видео в посте (10).
        // TODO: сделать ограничение на постинг либо только фото,
        //       либо только видео в одном посте.
        // TODO: сделать ограничение на количество видосов в посте (1)
        // TODO: прописать ограничения на размер видоса (20 мб с учетом телеги)

        // если медиа содержит видосы, то загружаем только их
        if (post.Media.Where(m => m.Type == MediaType.Video).Any())
        {
            var video = post.Media
                .Where(m => m.Type == MediaType.Video)
                .First();

            var saveResponse = api.Video.Save(new VideoSaveParams
            {
                // TODO: вынести эту строку в конфиг
                Name = settingsService.VkVideoTitle,
                //Description = caption,
                IsPrivate = false,
                GroupId = groupId,
                NoComments = true,
                Wallpost = false
            });

            var request = new RestRequest(saveResponse.UploadUrl)
                .AddFile("video_file", fileService.Get(video.Id).PhysicalPath);

            request.AlwaysMultipartFormData = true;
            var response = await new RestClient().PostAsync<VideoUploadResult>(request);

            api.Wall.Post(new WallPostParams
            {
                OwnerId = -groupId,
                Message = post.Comment,
                Attachments = new List<MediaAttachment>
                {
                    new VkNet.Model.Attachments.Video
                    {
                        Id = response!.video_id,
                        OwnerId = -groupId
                    }
                },
                Signed = false,
                Copyright = post.SourceLink
            });
        }
        else
        {
            // первые 5 фоток
            var pictures = post.Media
                .Take(5)
                .Where(m => m.Type == MediaType.Picture)
                .ToList();

            var uploadServer = api.Photo.GetUploadServer(album.Id, groupId);

            var request = new RestRequest(uploadServer.UploadUrl)
            {
                AlwaysMultipartFormData = true
            };

            for (int i = 0; i < pictures.Count; i++)
            {
                request.AddFile(
                    $"file{i + 1}",
                    fileService.Get(pictures[i].Id).PhysicalPath);
            }

            var response = await new RestClient().PostAsync(request);

            var uploadedPhotos = api.Photo.Save(new PhotoSaveParams
            {
                AlbumId = album.Id,
                GroupId = groupId,
                SaveFileResponse = response.Content,
                // честно, я вк рот ебал
                // вот скажите мне, нахуя он постит описание фотки как текст поста
                // ????????
                //Caption = caption
            }).ToList();

            // оставшиеся 5 фоток
            pictures = post.Media
                .Skip(5)
                .Take(5)
                .Where(m => m.Type == MediaType.Picture)
                .ToList();

            if (pictures.Any())
            {
                uploadServer = api.Photo.GetUploadServer(album.Id, groupId);

                request = new RestRequest(uploadServer.UploadUrl)
                {
                    AlwaysMultipartFormData = true
                };

                for (int i = 0; i < pictures.Count; i++)
                {
                    request.AddFile(
                        $"file{i + 1}",
                        fileService.Get(pictures[i].Id).PhysicalPath);
                }

                response = await new RestClient().PostAsync(request);

                var uploadedPhotos2 = api.Photo.Save(new PhotoSaveParams
                {
                    AlbumId = album.Id,
                    GroupId = groupId,
                    SaveFileResponse = response.Content,
                    Caption = caption
                }).ToList();

                uploadedPhotos.AddRange(uploadedPhotos2);
            }

            api.Wall.Post(new WallPostParams
            {
                OwnerId = -groupId,
                Message = post.Comment,
                Attachments = uploadedPhotos,
                Signed = false,
                Copyright = post.SourceLink
            });
        }
    }

    List<IAlbumInputMedia> MakeTelegramInputMedia(Core.Entities.Post post)
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
                if (extension is FilesService.JPEG or FilesService.JPG)
                {
                    finalMedia = new InputMediaPhoto(media)
                    {
                        ParseMode = ParseMode.Markdown,
                        Caption = caption
                    };
                }
                else if (extension is FilesService.MP4)
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

                if (extension is FilesService.JPEG or FilesService.JPG)
                    finalMedia = new InputMediaPhoto(media);
                else if (extension is FilesService.MP4)
                    finalMedia = new InputMediaVideo(media) { SupportsStreaming = true };

                if (finalMedia != null)
                    result.Add(finalMedia);
            }
        }

        return result;
    }

    static int GetQuarter(DateTime date)
    {
        if (date.Month >= 1 && date.Month <= 3)
            return 1;
        else if (date.Month >= 4 && date.Month <= 6)
            return 2;
        else if (date.Month >= 7 && date.Month <= 9)
            return 3;
        else
            return 4;
    }

    class VideoUploadResult
    {
        public int size { get; set; }
        public int video_id { get; set; }
    }
}