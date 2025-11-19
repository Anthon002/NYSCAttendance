using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NYSCAttendance.Infrastructure.Data.Entities;

public sealed class LGA : BaseEntity
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }

    [Precision(10, 5)]
    public double DistanceInMeters { get; set; }

    [MaxLength(30)]
    public string Token { get; set; } = default!;

    [MaxLength(100)]
    public string Name { get; set; } = default!;
}

public sealed class CDS : BaseEntity
{
    public string Name { get; set; } = default!;
}
