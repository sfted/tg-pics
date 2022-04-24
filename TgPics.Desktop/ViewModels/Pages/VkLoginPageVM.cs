namespace TgPics.Desktop.ViewModels.Pages;

using DesktopKit.MVVM;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

public class VkLoginPageVM : ViewModelBase
{
    public VkLoginPageVM() =>
        AuthUrl = new Uri($"https://oauth.vk.com/authorize?client_id={VK_APP_ID}&display=popup&" +
                $"redirect_uri=https://oauth.vk.com/blank.html&scope={SETTINGS}&response_type=token&v={API_VERSION}");

    public const string VK_APP_ID = "8068005";
    public const string SETTINGS = "photos";
    public const string API_VERSION = "5.131";

    private Uri authUrl;

    public event Action LoginCompleted;

    public Uri AuthUrl
    {
        get => authUrl;
        set => SetProperty(ref authUrl, value);
    }

    public string Token { get; private set; }

    public void ProceedLogin(string url)
    {
        if (url.Contains("access_token="))
        {
            Token = GetTokenFromResponse(url);
            Debug.WriteLine(url);
            Debug.WriteLine(Token);
            LoginCompleted?.Invoke();
        }
    }

    private static string GetTokenFromResponse(string response)
        => Regex.Match(response, @"access_token=(.*?)&expires_in").Groups[1].Value;
}