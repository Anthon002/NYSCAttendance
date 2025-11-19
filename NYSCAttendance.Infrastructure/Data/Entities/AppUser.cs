using Microsoft.AspNetCore.Identity;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.Data.Entities;

public sealed class AppUser : IdentityUser<long>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public UserTypeEnum UserType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class AppRole : IdentityRole<long>;
public sealed class AppUserClaim : IdentityUserClaim<long>;