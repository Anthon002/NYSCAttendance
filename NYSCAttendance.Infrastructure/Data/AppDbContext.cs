using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data.Entities;

namespace NYSCAttendance.Infrastructure.Data;

public sealed class AppDbContext : IdentityDbContext<AppUser, AppRole, long>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {

    }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<AppRole> AppRoles { get; set; }
    public DbSet<LGA> LGAs { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<AppUserClaim> AppUserClaims { get; set; }
    public DbSet<OTP> OTPs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Seed();
    }
}
