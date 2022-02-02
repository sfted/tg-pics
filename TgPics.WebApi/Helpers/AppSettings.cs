namespace TgPics.WebApi.Helpers;

public class AppSettings
{
    private int postsPerDay;

    public string WebApiKey { get; set; }
    public int PostPerDay
    {
        get => postsPerDay;
        set
        {
            postsPerDay = value;
            Timings = CalculateTimings(postsPerDay);
        }
    }

    public List<TimeSpan> Timings { get; private set; }
    
    public List<TimeSpan> CalculateTimings(int interval)
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

        while(current < TimeSpan.FromHours(24))
        {
            current = current.Add(offset);
            timings.Add(current);
        }

        return timings;
    }
}
