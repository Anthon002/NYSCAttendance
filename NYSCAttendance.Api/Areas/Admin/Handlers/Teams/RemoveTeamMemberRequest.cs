using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.Auth
{
    public sealed record RemoveTeamMemeberRequest : IRequest<BaseResponse>
    {
        internal long Id { get; set; }
    }

    public sealed class RemoveTeamMemeberRequestHandler : IRequestHandler<RemoveTeamMemeberRequest, BaseResponse>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RemoveTeamMemeberRequestHandler> _logger;
        private readonly INotificationService _notificationService;
        public RemoveTeamMemeberRequestHandler(AppDbContext context, ILogger<RemoveTeamMemeberRequestHandler> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }
        public async ValueTask<BaseResponse> Handle(RemoveTeamMemeberRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                if (user is null)
                    return new BaseResponse(false, "User not found.");

                var email = user.Email;
                var firstName = user.FirstName;

                _context.AppUsers.Remove(user);

                var userPermissions = await _context.AppUserClaims.Where(x => x.UserId == request.Id).ToArrayAsync(cancellationToken);

                _context.AppUserClaims.RemoveRange(userPermissions);

                await _context.SaveChangesAsync(cancellationToken);
                await _notificationService.AdminSendAccountRemovedNotificationAsync(new MailRequest {Email = email!, FirstName = firstName}, cancellationToken);
                return new BaseResponse(true, "User removed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_RemoveTeamMemeberRequest => Application ran into an error while trying to add a new team member.");
                return new BaseResponse(false, "Application ran into an error.");
            }
        }
    }
}