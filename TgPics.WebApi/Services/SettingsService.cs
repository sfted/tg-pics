namespace TgPics.WebApi.Services;

public interface ISettingsService
{
    public int PostsPerDay { get; }
    public string WebApiKey { get; }
    public string AdminPwd { get; }
    public string BotToken { get; }
    public string TgAdminChatId { get; }
    public string TgChannelUsername { get; }
    public List<TimeSpan> Timings { get; }
}

public class SettingsService : ISettingsService
{
    public SettingsService(
        int postsPerDay,
        string webApiKey,
        string adminPwd,
        string botToken,
        string tgAdminChatId,
        string tgChannelUsername)
    {
        PostsPerDay = postsPerDay;
        WebApiKey = webApiKey;
        AdminPwd = adminPwd;
        BotToken = botToken;
        TgAdminChatId = tgAdminChatId;
        TgChannelUsername = tgChannelUsername;

        Timings = CalculateTimings(PostsPerDay);
    }

    public int PostsPerDay { get; set; }
    public string WebApiKey { get; set; }
    public string AdminPwd { get; set; }
    public string BotToken { get; set; }
    public string TgAdminChatId { get; set; }
    public string TgChannelUsername { get; set; }
    public List<TimeSpan> Timings { get; private set; }

    static List<TimeSpan> CalculateTimings(int interval)
    {
        if (interval <= 0)
            throw new ArgumentException(
                "The interval must be greater than zero.", nameof(interval));

        if (interval > 24)
            throw new ArgumentException(
                "The interval must be less than 24.", nameof(interval));

        var timings = new List<TimeSpan>();
        var offset = TimeSpan.FromHours(24 / interval);
        var current = TimeSpan.Zero;

        while (current < TimeSpan.FromHours(24))
        {
            current = current.Add(offset);
            timings.Add(current);
        }

        return timings;
    }
}