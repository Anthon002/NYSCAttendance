using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.Data.Entities;

public sealed class OTP : BaseEntity
{
    public string Code { get; set; } = default!;
    public long UserId { get; set; }
    public string Identifier { get; set; } = default!;
    public OTPStatusEnum Status { get; set; }
}
