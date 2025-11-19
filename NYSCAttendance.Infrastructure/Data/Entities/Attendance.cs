using System.ComponentModel.DataAnnotations;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.Data.Entities;

public sealed class Attendance : BaseEntity
{
    [MaxLength(25)]
    public string Identifier { get; set; } = default!;

    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    [MaxLength(50)]
    public string? MiddleName { get; set; }
    
    [MaxLength(50)]
    public string? StateCode { get; set; }
    public BatchEnum Batch { get; set; }
    public long SerialNumber { get; set; }
    public long LGAId { get; set; }
    public int Day { get; set; }
    public bool ISReserve { get; set; }
    public CDSEnum CDS { get; set; }
}
