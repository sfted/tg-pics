namespace TgPics.WebApi.Helpers;

public static class Extensions
{
    public static string GetFullHost(this HttpRequest request)
    {
        return $"{request.Scheme}://{request.Host}";
    }
}