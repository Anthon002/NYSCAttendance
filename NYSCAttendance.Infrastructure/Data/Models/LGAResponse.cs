using System.ComponentModel.DataAnnotations;

namespace NYSCAttendance.Infrastructure.Data.Models;

public sealed record LGAResponse
{
    public string Name { get; set; } = default!;
    public long Id { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public double DistanceInMeters { get; set; }
    public string Token { get; set; } = default!;
    public string OpenTime { get; set; } = default!;
    public string CloseTime { get; set; } = default!;
}

public sealed record AttendanceResponse
{
    public long Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string StateCode { get; set; } = default!;
    public DateTimeOffset RecordedAt { get; set; }
    public string? Day { get; set; }
    public long SerialNumber { get; set; } = default!;
    public string Identifier { get; set; } = default!;
    public string CDS { get; set; } = default!;
    public int DayInt;
}

