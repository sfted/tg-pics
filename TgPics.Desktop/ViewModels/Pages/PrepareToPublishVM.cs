namespace TgPics.Desktop.ViewModels.Pages;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TgPics.Core.Models.Requests;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM;
using TgPics.Api.Client;
using DesktopKit.Services;
using Microsoft.UI.Xaml.Controls;
using TgPics.Desktop.Services;
using TgPics.Desktop.Values;

internal interface IPrepareToPublishVM
{
    string Comment { get; set; }
    PrepareToPublishPostViewModel Post { get; set; }
    RelayCommand PublishCommand { get; }
    List<PrepareToPublishPhotoViewModel> SelectedPhotos { get; set; }
}

internal class PrepareToPublishVM : ViewModelBase, IPrepareToPublishVM
{
    public PrepareToPublishVM(
        INavigationService navigationService,
        ISettingsService settingsService,
        PrepareToPublishPostViewModel post)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;

        Post = post;
        PublishCommand = new(Publish);
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;

    private List<PrepareToPublishPhotoViewModel> selectedPhotos;
    private string comment;

    public PrepareToPublishPostViewModel Post { get; set; }

    public List<PrepareToPublishPhotoViewModel> SelectedPhotos
    {
        get => selectedPhotos;
        set => SetProperty(ref selectedPhotos, value);
    }

    public string Comment
    {
        get => comment;
        set => SetProperty(ref comment, value);
    }

    public RelayCommand PublishCommand { get; private set; }

    private async void Publish()
    {
        // TODO: ПЕРЕПИСАТЬ ВСЁ!!!
        // по-хорошему, зачем я сохраняю файл на пк? если я могу просто в озу сохранить...
        if (SelectedPhotos != null && SelectedPhotos.Count > 0)
        {
            var tempDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "tgpicstemp");

            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            var paths = new List<string>();
            using (var web = new WebClient())
            {
                var counter = 0;
                SelectedPhotos.ForEach(p =>
                    {
                        var path = $"{tempDir}//image_{counter++}.jpg";
                        web.DownloadFile(p.Model.OriginalUrl, path);
                        paths.Add(path);
                    }
                );
            }

            var client = new TgPicsApi(
                settingsService.Get<string>(SettingsKeys.TG_PICS_HOST),
                settingsService.Get<string>(SettingsKeys.TG_PICS_TOKEN),
                secure: false);

            var response = await client.UploadFilesAsync(paths);

            var request = new PostsAddRequest
            {
                SourceLink = Post.Model.SourceLink.ToString(),
                SourcePlatfrom = "vk",
                SourceTitle = Post.Model.SourceTitle,
                Comment = Comment,
                MediaIds = response.Items.Select(f => f.Id).ToList()
            };

            try
            {
                var result = await client.AddPostAsync(request);
                Post.SetTagToOriginalPost();
                await navigationService.ShowDialogAsync(
                    new ContentDialog().MakeSuccessful(
                        $"Пост успешно добавлен в очередь!\n" +
                        $"Дата и время публикации: {result.PublicationDateTime}\n" +
                        $"Id: {result.Id}"));
            }
            catch (Exception ex)
            {
                await navigationService.ShowDialogAsync(new ContentDialog().MakeException(ex));
            }
        }
    }
}