namespace TgPics.Desktop.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using TgPics.Desktop.MVVM;
using TgPics.Desktop.MVVM.Interfaces;
using VkNet.Model;
using VkNet.Model.Attachments;
using static TgPics.Desktop.ViewModels.Pages.VkBookmarksPageViewModel;

public class FaveGetObjectViewModel : ViewModelBase, IModel<FaveGetObjectButBetter>
{
    public FaveGetObjectViewModel(
        FaveGetObjectButBetter model,
        List<User> profiles,
        List<Group> groups)
    {
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

            Photos = Model.Post.Attachments
                .Where(a => a.Type == typeof(Photo))
                .Select(p => new PhotoViewModel(p.Instance as Photo))
                .ToList();

            Url = new Uri($"https://vk.com/wall{model.Post.OwnerId}_{model.Post.Id}");

            OpenInBrowserCommand = new(OpenInBrowser);
        }
    }

    public FaveGetObjectButBetter Model { get; set; }
    public string GroupName { get; set; }
    public DateTime Date { get; set; }
    public Uri GroupProfilePic { get; set; }
    public string AuthorName { get; set; }
    public bool IsSigned { get; private set; } = true;
    public List<PhotoViewModel> Photos { get; set; }
    public Uri Url { get; set; }

    public RelayCommand OpenInBrowserCommand { get; private set; }

    private void OpenInBrowser() =>
        Windows.System.Launcher.LaunchUriAsync(Url);
}