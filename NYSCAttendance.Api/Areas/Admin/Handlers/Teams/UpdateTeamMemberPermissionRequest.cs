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
    public sealed record UpdateTeamMemberPermissionRequest : IRequest<BaseResponse>
    {
        internal long Id { get; set; }
        public string[]? AssignPermissions { get; set; }
        public string[]? UnassignPermissions { get; set; }
    }

    public sealed class UpdateTeamMemberPermissionRequestHandler : IRequestHandler<UpdateTeamMemberPermissionRequest, BaseResponse>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UpdateTeamMemberPermissionRequestHandler> _logger;
        private readonly INotificationService _notificationService;
        public UpdateTeamMemberPermissionRequestHandler(AppDbContext context, ILogger<UpdateTeamMemberPermissionRequestHandler> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }
        public async ValueTask<BaseResponse> Handle(UpdateTeamMemberPermissionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context.AppUsers.Where(x => x.Id == request.Id).Select(x => new {x.UserType}).FirstOrDefaultAsync(cancellationToken);
                if (user is null)
                    return new BaseResponse(false, "User not found.");
                
                if (user.UserType == UserTypeEnum.SuperAdmin)
                    return new BaseResponse(false, "This user can not be updated.");

                var usersPermissions = await _context.AppUserClaims.Where(x => x.UserId == request.Id).Select(x => x.ClaimValue).ToArrayAsync(cancellationToken);

                if (request.UnassignPermissions != null && request.UnassignPermissions.Any(x => !usersPermissions.Contains(x)))
                    return new BaseResponse(false, "This user does not have the permission that you are removing.");

                if (request.AssignPermissions != null && request.AssignPermissions.Any(x => usersPermissions.Contains(x)))
                    return new BaseResponse(false, "This user already has the permission that you are assigning.");

                using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        if (request.UnassignPermissions != null)
                        {
                            var removePermissions = await _context.AppUserClaims.Where(x => request.UnassignPermissions.Contains(x.ClaimValue) && x.UserId == request.Id).ToArrayAsync(cancellationToken);
                            _context.AppUserClaims.RemoveRange(removePermissions);
                        }

                        if (request.AssignPermissions != null)
                        {
                            var addClaims = request.AssignPermissions.Select(x => new AppUserClaim
                            {
                                UserId = request.Id,
                                ClaimType = AppConstants.Permission,
                                ClaimValue = x
                            }).ToArray();

                            await _context.AppUserClaims.AddRangeAsync(addClaims, cancellationToken);
                        }

                        await _context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                        return new BaseResponse(true, "Admin permission updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogError(ex, "Admin_UpdateTeamMemberPermissionRequest => Application ran into an error when trying to update admin's permissions.");
                        return new BaseResponse(false, "Application ran into an error.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_UpdateTeamMemberPermissionRequest => Application ran into an error while trying to add a new team member.");
                return new BaseResponse(false, "Application ran into an error.");
            }
        }
    }
}