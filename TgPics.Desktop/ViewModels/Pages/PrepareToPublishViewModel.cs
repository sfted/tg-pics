using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TgPics.Core.Models.Requests;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM;
using TgPics.Api.Client;

namespace TgPics.Desktop.ViewModels.Pages;

public class PrepareToPublishViewModel : ViewModelBase
{
    public PrepareToPublishViewModel(PrepareToPublishPostViewModel post)
    {
        Post = post;
        PublishCommand = new(Publish);
    }

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
        if (SelectedPhotos != null && SelectedPhotos.Count > 0)
        {
            var tempDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "tgpicstemp");

            if(!Directory.Exists(tempDir))
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
                Settings.Instance.Get<string>(SettingsVM.TG_PICS_HOST),
                Settings.Instance.Get<string>(SettingsVM.TG_PICS_TOKEN),
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

            //try
            //{
            //    var result = await client.AddPostAsync(request);
            //    Post.SetTagToOriginalPost();
            //    await App.ShowSuccessfulDialog($"Пост успешно добавлен в очередь!\n" +
            //        $"Дата и время публикации: {result.PublicationDateTime}\n" +
            //        $"Id: {result.Id}");
            //}
            //catch (Exception ex)
            //{
            //    await App.ShowExceptionDialog(ex);
            //}
        }
    }
}