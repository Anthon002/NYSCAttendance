using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.Auth
{
    public sealed record AddTeamMemberRequest : IRequest<BaseResponse>
    {
        public string Email { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string[]? Permissions { get; set; }
    }

    public sealed class AddTeamMemberRequestHandler : IRequestHandler<AddTeamMemberRequest, BaseResponse>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AddTeamMemberRequestHandler> _logger;
        private readonly IPasswordHasher<AppUser> _passwordHaser;
        private readonly IUtilityService _utilityService;
        private readonly INotificationService _notificationService;
        public AddTeamMemberRequestHandler(AppDbContext context, ILogger<AddTeamMemberRequestHandler> logger, IPasswordHasher<AppUser> passwordHasher, IPasswordHasher<AppUser> passwordHaser, IUtilityService utilityService, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _passwordHaser = passwordHaser;
            _utilityService = utilityService;
            _notificationService = notificationService;
        }
        public async ValueTask<BaseResponse> Handle(AddTeamMemberRequest request, CancellationToken cancellationToken)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    if (await _context.AppUsers.AnyAsync(x => x.Email == request.Email.Trim().ToLower()))
                        return new BaseResponse(false, "A user with this email already exists.");

                    // create AppUser object
                    var user = new AppUser
                    {
                        FirstName = request.FirstName.Trim(),
                        LastName = request.LastName.Trim(),
                        Email = request.Email.ToLower().Trim(),
                        UserName = request.Email.ToLower(),
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        UserType = UserTypeEnum.Admin,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    await _context.AppUsers.AddAsync(user, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    // assign permissions
                    if (request.Permissions != null)
                    {
                        foreach (var permission in request.Permissions)
                        {
                            switch (permission)
                            {
                                case AppConstants.TeamManagement:
                                    await _context.AppUserClaims.AddAsync(new AppUserClaim
                                    {
                                        ClaimType = AppConstants.Permission,
                                        ClaimValue = AppConstants.TeamManagement,
                                        UserId = user.Id
                                    }, cancellationToken);
                                    break;
                                case AppConstants.LGAManagement:
                                    await _context.AppUserClaims.AddAsync(new AppUserClaim
                                    {
                                        ClaimType = AppConstants.Permission,
                                        ClaimValue = AppConstants.TeamManagement,
                                        UserId = user.Id
                                    }, cancellationToken);
                                    break;
                                default:
                                    await transaction.RollbackAsync(cancellationToken);
                                    return new BaseResponse(false, "An invalid permission was provided. Please check and try again.");
                            }
                        }
                    }

                    // generate password
                    var password = _utilityService.GeneratePassword(12);
                    var passwordHash = _passwordHaser.HashPassword(user, password);
                    user.PasswordHash = passwordHash;
                    await _context.SaveChangesAsync(cancellationToken);

                    // send credentials
                    await _notificationService.AdminSendLoginCredentialsNotificationAsync(new MailRequest
                    {
                        Email = user.Email,
                        FirstName = user.FirstName
                    }, password, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    return new BaseResponse(true, "Team member created successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Admin_AddTeamMemberRequest => Application ran into an error while trying to add a new team member.");
                    return new BaseResponse(false, "Application ran into an error.");
                }
            }
        }
    }
}