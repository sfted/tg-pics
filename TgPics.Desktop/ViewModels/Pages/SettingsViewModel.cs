namespace TgPics.Desktop.ViewModels.Pages;

using Microsoft.UI.Xaml.Controls;
using System;
using TgPics.Core.Models;
using TgPics.Desktop.Helpers;
using TgPics.Desktop.MVVM;
using TgPics.Desktop.Views.Pages;
using TgPics.WebApiWrapper;

public class SettingsViewModel : ViewModelBase
{
    public SettingsViewModel()
    {
        SaveHostCommand = new(SaveHost);
        LogInTgPicsCommand = new(LogInTgPics);
        ProceedLoginCommand = new(ProceedLogin);

        TgPicsHost = settings.Get<string>(TG_PICS_HOST);
        TgPicsUsername = settings.Get<string>(TG_PICS_USERNAME);

        if (!string.IsNullOrEmpty(settings.Get<string>(TG_PICS_TOKEN)))
            TgPicsIsLoggedIn = true;
    }

    private const string TG_PICS_HOST = "tg_pics_host";
    private const string TG_PICS_TOKEN = "tg_pics_token";
    private const string TG_PICS_USERNAME = "tg_pics_username";

    private readonly Settings settings = Settings.Instance;

    private string tgPicsHost = string.Empty;
    private string tgPicsUsername = string.Empty;
    private bool tgPicsIsLoggedIn = false;

    public string TgPicsHost
    {
        get => tgPicsHost;
        set => SetProperty(ref tgPicsHost, value);
    }

    public bool TgPicsIsLoggedIn
    {
        get => tgPicsIsLoggedIn;
        set => SetProperty(ref tgPicsIsLoggedIn, value);
    }

    public string TgPicsUsername
    {
        get => tgPicsUsername;
        set => SetProperty(ref tgPicsUsername, value);
    }

    public RelayCommand SaveHostCommand { get; private set; }
    public RelayCommand LogInTgPicsCommand { get; private set; }
    public RelayCommand<LoginPage> ProceedLoginCommand { get; private set; }

    private void SaveHost() =>
        settings.Set(TG_PICS_HOST, TgPicsHost);

    private async void LogInTgPics()
    {
        var page = new LoginPage();
        var dialog = new ContentDialog()
        {
            XamlRoot = App.XamlRoot,
            Title = "Вход в аккаунт",
            Content = page,
            PrimaryButtonText = "Войти",
            PrimaryButtonCommand = ProceedLoginCommand,
            DefaultButton = ContentDialogButton.Primary,
            CloseButtonText = "Отмена",
            PrimaryButtonCommandParameter = page
        };

        await dialog.ShowAsync();
    }

    private async void ProceedLogin(LoginPage page)
    {
        var client = new TgPicsClient(TgPicsHost);
        var request = new AuthenticateRequest
        {
            Username = page.ViewModel.Username,
            Password = page.ViewModel.Password
        };

        try
        {
            var response = await client.AuthAsync(request);
            settings.Set(TG_PICS_TOKEN, response.Token);
            settings.Set(TG_PICS_USERNAME, response.Username);
            TgPicsIsLoggedIn = true;
            NotifyPropertyChanged(TgPicsUsername);
        }
        catch (Exception ex)
        {
            var dialog = new ContentDialog()
            {
                XamlRoot = App.XamlRoot,
                Title = "Ошибка :(",
                Content = ex.Message,
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            };

            await dialog.ShowAsync();
        }
    }
}