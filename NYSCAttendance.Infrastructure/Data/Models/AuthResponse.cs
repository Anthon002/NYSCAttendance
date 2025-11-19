namespace NYSCAttendance.Infrastructure.Data.Models;

public sealed record LoginResponse
{
    public string Email { get; set; } = default!;
    public string Token { get; set; } = default!;
    public long Id { get; set; }
    public DateTimeOffset ExpiresTime { get; set; }
}


