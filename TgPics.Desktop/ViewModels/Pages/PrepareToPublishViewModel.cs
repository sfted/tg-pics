namespace TgPics.Desktop.ViewModels.Pages;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TgPics.Core.Models;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM;
using TgPics.WebApiWrapper;

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
        if(SelectedPhotos != null && SelectedPhotos.Count > 0)
        {
            Post.SetTagToOriginalPost();

            var request = new AddPostRequest
            {
                SourceLink = Post.Model.SourceLink.ToString(),
                SourceTitle = Post.Model.SourceTitle,
                Comment = Comment,
                MediaIds = SelectedPhotos.Select(p => new PictureInfo
                {
                    Url = p.Model.OriginalUrl.ToString()
                })
            };

            Debug.WriteLine(JsonConvert.SerializeObject(request));

            var client = new TgPicsClient(
                Settings.Instance.Get<string>(SettingsViewModel.TG_PICS_HOST),
                Settings.Instance.Get<string>(SettingsViewModel.TG_PICS_TOKEN));

            await client.AddPostAsync(request);
        }
    }
}