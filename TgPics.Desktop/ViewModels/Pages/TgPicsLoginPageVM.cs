namespace TgPics.Desktop.ViewModels.Pages;

using DesktopKit.MVVM;

internal interface ITgPicsLoginPageVM
{
    string Host { get; set; }
    string Password { get; set; }
    string Username { get; set; }
}

internal class TgPicsLoginPageVM : ViewModelBase, ITgPicsLoginPageVM
{
    string host;
    string username;
    string password;

    public string Host
    {
        get => host;
        set => SetProperty(ref host, value);
    }

    public string Username
    {
        get => username;
        set => SetProperty(ref username, value);
    }

    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }
}