namespace NYSCAttendance.Infrastructure.Utils;

public sealed class AppSettingsOptions
{
    public JWTSettingsOption? JWTSettings { get; set; }
    public Brevo? Brevo { get; set; }
    public AppSettings? AppSettings { get; set; }
}

public sealed class AppSettings
{
    public string AppName { get; set; } = default!;
    public string SupportEmail { get; set; } = default!;
    public string AdminUrl { get; set; } = default!;

} 

public sealed class JWTSettingsOption
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public int ExpiryTime { get; set; }
}

public sealed class Brevo
{
    public string Baseurl { get; set; } = default!;
    public string Secret { get; set; } = default!;
}