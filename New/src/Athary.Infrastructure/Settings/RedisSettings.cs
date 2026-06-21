namespace Athary.Infrastructure.Settings;

public sealed class RedisSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "Athary";
}
