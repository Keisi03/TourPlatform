namespace TourPlatform.Infrastructure.Authorization.Settings;
public class RedisSettings
{
    public string Connection { get; set; } = string.Empty;
    public int JtiExpiryMinutes { get; set; }
}