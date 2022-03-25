namespace TgPics.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using TgPics.Core.Models;
using TgPics.Desktop.MVVM;
using TgPics.Desktop.MVVM.Interfaces;
using TgPics.Desktop.Utils.Extensions;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using static TgPics.Desktop.ViewModels.Pages.VkBookmarksViewModel;

public class FaveGetObjectViewModel : ViewModelBase, IModel<FaveGetObjectButBetter>
{
    public FaveGetObjectViewModel(
        VkApi api,
        FaveGetObjectButBetter model,
        List<User> profiles,
        List<Group> groups)
    {
        this.api = api;
        Model = model;

        if (model.Post != null)
        {
            var group = groups
                .FirstOrDefault(g => g.Id == -model.Post.OwnerId);

            if (group != null)
            {
                GroupName = group.Name;
                GroupProfilePic = group.Photo100;
            }

            // в какой-то момент дата перестала нормально парситься из json'а
            Date = DateTime
                .ParseExact("01-01-1970", "dd-MM-yyyy", null)
                .AddSeconds(model.Post.DateSeconds);

            var author = profiles
                .FirstOrDefault(p => p.Id == model.Post.SignerIdSignerIdButNotAFuckingNull);

            if (author != null)
                AuthorName = $"{author.FirstName} {author.LastName}";
            else
                IsSigned = false;

            if (!string.IsNullOrEmpty(Model.Post.Text))
                Text = Model.Post.Text;
            else
                HasText = false;

            Photos = Model.Post.Attachments
                .Where(a => a.Type == typeof(Photo))
                .Select(p => new PhotoViewModel(p.Instance as Photo))
                .ToList();

            Tags = Model.Tags.ToList();

            Url = new Uri($"https://vk.com/wall{model.Post.OwnerId}_{model.Post.Id}");

            OpenInBrowserCommand = new(OpenInBrowser);
            PrepareToPublishCommand = new(PrepareToPublish);
        }
    }

    private readonly VkApi api;

    public FaveGetObjectButBetter Model { get; set; }
    public string GroupName { get; set; }
    public DateTime Date { get; set; }
    public Uri GroupProfilePic { get; set; }
    public string Text { get; set; }
    public string AuthorName { get; set; }
    public bool IsSigned { get; set; } = true;
    public bool HasText { get; set; } = true;
    public List<PhotoViewModel> Photos { get; set; }
    public List<FaveTag> Tags { get; set; }
    public Uri Url { get; set; }

    public RelayCommand OpenInBrowserCommand { get; private set; }
    public RelayCommand PrepareToPublishCommand { get; private set; }

    private void OpenInBrowser() =>
        Windows.System.Launcher.LaunchUriAsync(Url);

    private void PrepareToPublish() =>
        Desktop.App.NavgateTo(
            "prepare_to_publish",
            new PrepareToPublishPostViewModel(api, new PrepareToPublishPost
            {
                SourceLink = Url,
                SourceTitle = $"{GroupName}",
                Photos = Photos.Select(p => new Core.Models.PrepareToPublishPhoto
                {
                    OriginalUrl = p.OriginalUri,
                    Preview32Url = p.Model.GetSmallest32AspectRatioImageUri()
                }).ToList()
            }));
}