using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using TgPics.Desktop.MVVM;
using TgPics.Desktop.Views.Pages;
using Windows.Storage;

namespace TgPics.Desktop.ViewModels.Pages
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            LogInTelegramCommand = new(LogInTelegram);
            //TestTelegramCommand = new(Test);
            //TestChatIdCommand = new(TestChatId);
        }

        const string APP_ID = "app_id";
        const string APP_HASH = "app_hash";

        private readonly ApplicationDataContainer settings =
            ApplicationData.Current.LocalSettings;

        private bool telegramIsLoggedIn = true;
        private bool telegramChannelIsSelected = true;

        public bool TelegramIsLoggedIn
        {
            get => telegramIsLoggedIn;
            set => SetProperty(ref telegramIsLoggedIn, value);
        }

        public bool TelegramChannelIsSelected
        {
            get => telegramChannelIsSelected;
            set => SetProperty(ref telegramChannelIsSelected, value);
        }

        public string AppId
        {
            get
            {
                if (settings.Values[APP_ID] != null)
                    return settings.Values[APP_ID] as string;
                else
                    return string.Empty;
            }

            set
            {
                settings.Values[APP_ID] = value;
            }
        }

        public string AppHash
        {
            get
            {
                if (settings.Values[APP_HASH] != null)
                    return settings.Values[APP_HASH] as string;
                else
                    return string.Empty;
            }

            set
            {
                settings.Values[APP_HASH] = value;
            }
        }

        public RelayCommand LogInTelegramCommand { get; private set; }
        public RelayCommand LogOutOfTelegramCommand { get; private set; }
        public RelayCommand SelectTelegramChannelCommand { get; private set; }
        //public RelayCommand LogOutOfTelegramCommand { get; private set; }

        private void LogInTelegram()
        {
            TelegramIsLoggedIn = !TelegramIsLoggedIn;
        }

        private async void Test()
        {
            var codePage = new TwoFACodePage();
            var dialog = new ContentDialog
            {
                XamlRoot = App.XamlRoot,
                Title = "2FA",
                Content = codePage,
                CloseButtonText = "ОК",
                CloseButtonCommand = new RelayCommand(() => Debug.WriteLine(codePage.Code))
            };

            await dialog.ShowAsync();
        }

        private async void TestTelegram()
        {
            try
            {
                //var bot = new TelegramBotClient(BotToken);
                //var info = await bot.GetMeAsync();
                //
                //var dialog = new ContentDialog
                //{
                //    XamlRoot = App.XamlRoot,
                //    Title = "Успешно!",
                //    Content = $"Юзернейм бота: @{info.Username}",
                //    CloseButtonText = "ОК",
                //};
                //
                //await dialog.ShowAsync();
                //await bot.CloseAsync();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    XamlRoot = App.XamlRoot,
                    Title = "Ошибка :(",
                    FontSize = 12,
                    Content = $"Сообщение об ошибке: {ex.Message}",
                    CloseButtonText = "ОК",
                };

                await dialog.ShowAsync();
            }
        }
    }
}
