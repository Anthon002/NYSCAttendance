using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.Data.Models;

public record class TeamMembersResponse
{
    public long Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public UserTypeEnum UserType { get; set; }
    public string[]? Permissions { get; set; }
}
