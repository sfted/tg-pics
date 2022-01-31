using TgPics.Desktop.MVVM;

namespace TgPics.Desktop.ViewModels.Pages;

public class LoginPageViewModel : ViewModelBase
{
    private string username;
    private string password;

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