namespace TgPics.Desktop.ViewModels;

using global::Windows.System;
using TgPics.Core.Models;
using TgPics.Desktop.MVVM;
using TgPics.Desktop.MVVM.Interfaces;

public class PrepareToPublishPhotoViewModel : IModel<PrepareToPublishPhoto>
{
    public PrepareToPublishPhotoViewModel(PrepareToPublishPhoto photo)
    {
        Model = photo;
        OpenInBrowserCommand = new(OpenInBrowser);
    }

    public PrepareToPublishPhoto Model { get; set; }

    public RelayCommand OpenInBrowserCommand { get; private set; }

    private void OpenInBrowser() =>
        Launcher.LaunchUriAsync(Model.OriginalUrl);
}