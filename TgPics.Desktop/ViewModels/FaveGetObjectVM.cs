namespace TgPics.Desktop.ViewModels;

using DesktopKit.MVVM;
using DesktopKit.MVVM.Interfaces;
using DesktopKit.Services;
using global::Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TgPics.Core.Models;
using TgPics.Desktop.Services;
using TgPics.Desktop.Utils.Extensions;
using TgPics.Desktop.Views.Pages;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using static TgPics.Desktop.ViewModels.Pages.VkBookmarksVM;

internal interface IFaveGetObjectVM
{
    string AuthorName { get; set; }
    DateTime Date { get; set; }
    string GroupName { get; set; }
    Uri GroupProfilePic { get; set; }
    bool HasText { get; set; }
    bool IsSigned { get; set; }
    FaveGetObjectButBetter Model { get; set; }
    Command OpenInBrowserCommand { get; }
    List<PhotoViewModel> Photos { get; set; }
    Command PrepareToPublishCommand { get; }
    List<FaveTag> Tags { get; set; }
    string Text { get; set; }
    Uri Url { get; set; }
}

internal class FaveGetObjectVM : ViewModelBase, IModel<FaveGetObjectButBetter>, IFaveGetObjectVM
{
    public FaveGetObjectVM(
        INavigationService navigationService,
        ISettingsService settingsService,
        IVkApiService vkApiService,
        FaveGetObjectButBetter model,
        List<VkNet.Model.User> profiles,
        List<Group> groups)
    {
        this.navigationService = navigationService;
        this.settingsService = settingsService;
        this.vkApiService = vkApiService;
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

            try
            {
                Photos = Model.Post.Attachments
                    .Where(a => a.Type == typeof(Photo))
                    .Select(p => new PhotoViewModel(p.Instance as Photo))
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            Tags = Model.Tags.ToList();

            Url = new Uri($"https://vk.com/wall{model.Post.OwnerId}_{model.Post.Id}");

            OpenInBrowserCommand = new(OpenInBrowser);
            PrepareToPublishCommand = new(PrepareToPublish);
        }
    }

    readonly INavigationService navigationService;
    readonly ISettingsService settingsService;
    readonly IVkApiService vkApiService;

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

    public Command OpenInBrowserCommand { get; private set; }
    public Command PrepareToPublishCommand { get; private set; }

    private void OpenInBrowser() => Launcher.LaunchUriAsync(Url);

    // TODO: переписать
    private async void PrepareToPublish()
    {
        var vm = new PrepareToPublishPostViewModel(settingsService, vkApiService, new PrepareToPublishPost
        {
            SourceLink = Url,
            SourceTitle = $"{GroupName}",
            Photos = Photos.Select(p => new PrepareToPublishPhoto
            {
                OriginalUrl = p.OriginalUri,
                Preview32Url = p.Model.GetSmallest32AspectRatioImageUri()
            }).ToList()
        });

        var page = new PrepareToPublishPage
        {
            ViewModel = new(navigationService, settingsService, vm)
        };

        var scroll = new ScrollViewer
        {
            Content = page,
            Padding = new Thickness(0, 0, 4, 0),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var dialog = new ContentDialog()
        {
            Title = "Подготовка к публикации",
            Content = scroll,
            CloseButtonText = "Отмена",
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = page.ActionName,
            PrimaryButtonCommand = page.Action,
            DefaultButton = ContentDialogButton.Primary,
        };

        await navigationService.ShowDialogAsync(dialog);
    }
}