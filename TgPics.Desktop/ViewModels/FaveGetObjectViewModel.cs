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
        }
    }

    string groupName;
    DateTime date;
    Uri groupProfilePic;
    List<PhotoViewModel> photos;
    string authorName;

    public FaveGetObjectButBetter Model { get; set; }

    public string GroupName
    {
        get => groupName;
        set => SetProperty(ref groupName, value);
    }

    public DateTime Date
    {
        get => date;
        set => SetProperty(ref date, value);
    }

    public Uri GroupProfilePic
    {
        get => groupProfilePic;
        set => SetProperty(ref groupProfilePic, value);
    }

    public List<PhotoViewModel> Photos
    {
        get => photos;
        set => SetProperty(ref photos, value);
    }

    public string AuthorName
    {
        get => authorName;
        set => SetProperty(ref authorName, value);
    }

    public bool IsSigned { get; private set; } = true;
}