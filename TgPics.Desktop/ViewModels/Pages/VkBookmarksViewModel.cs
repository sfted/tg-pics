namespace TgPics.Desktop.ViewModels.Pages;

using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Utils;

public class VkBookmarksViewModel : ViewModelBase
{
    public VkBookmarksViewModel()
    {
        LoadMoreCommand = new(LoadMore);
        InitVkApi();
        LoadBookmarks();
    }

    VkApi api;
    int offset = 0;

    public ObservableCollection<FaveGetObjectViewModel> Items { get; private set; } = new();

    public RelayCommand LoadMoreCommand { get; private set; }

    private async void InitVkApi()
    {
        try
        {
            api = new VkApi();
            api.Authorize(new ApiAuthParams
            {
                AccessToken = Settings.Instance.Get<string>(
                    SettingsViewModel.VK_TOKEN),
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            var dialog = new ContentDialog()
            {
                XamlRoot = Desktop.App.XamlRoot,
                Title = "Ошибка :(",
                Content = ex.Message,
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            };

            await dialog.ShowAsync();
        }
    }


    private async void LoadBookmarks()
    {
        try
        {
            var parameters = new VkParameters
            {
                { "count", 50 },
                { "extended", 1 },
                { "offset", offset },
                { "item_type", "post" },
                { "access_token", api.Token },
                { "v", VkLoginPageViewModel.API_VERSION }
            };

            var scheme = new
            {
                Response = new
                {
                    Count = 0,
                    Items = new List<FaveGetObjectButBetter>(),
                    Profiles = new List<User>(),
                    Groups = new List<Group>(),
                },
            };

            var json = await api.InvokeAsync("fave.get", parameters);
            var response = JsonConvert.DeserializeAnonymousType(json, scheme);

            foreach (var item in response.Response.Items)
                Items.Add(new FaveGetObjectViewModel(
                    api,
                    item,
                    response.Response.Profiles,
                    response.Response.Groups));

            offset += response.Response.Items.Count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            var dialog = new ContentDialog()
            {
                XamlRoot = Desktop.App.XamlRoot,
                Title = "Ошибка :(",
                Content = ex.Message,
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            };

            await dialog.ShowAsync();
        }
    }

    private void LoadMore() => LoadBookmarks();


    public class FaveGetObjectButBetter : FaveGetObject
    {
        [JsonProperty("post")]
        public new PostButBetter Post { get; set; }
    }

    public class PostButBetter : Post
    {
        [JsonProperty("signer_id")]
        public long? SignerIdSignerIdButNotAFuckingNull { get; set; }

        [JsonProperty("date")]
        public double DateSeconds { get; set; }
    }
}