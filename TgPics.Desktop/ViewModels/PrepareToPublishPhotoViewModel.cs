namespace TgPics.Desktop.ViewModels;

using DesktopKit.MVVM;
using DesktopKit.MVVM.Interfaces;
using global::Windows.System;
using TgPics.Core.Models;

public class PrepareToPublishPhotoViewModel : IModel<PrepareToPublishPhoto>
{
    public PrepareToPublishPhotoViewModel(PrepareToPublishPhoto photo)
    {
        Model = photo;
        OpenInBrowserCommand = new(OpenInBrowser);
    }

    public PrepareToPublishPhoto Model { get; set; }

    public Command OpenInBrowserCommand { get; private set; }

    private void OpenInBrowser() =>
        Launcher.LaunchUriAsync(Model.OriginalUrl);
}