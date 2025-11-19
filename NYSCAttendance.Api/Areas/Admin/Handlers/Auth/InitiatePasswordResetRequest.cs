using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.JWTHandler;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.Auth
{
    public sealed record InitiatePasswordResetRequest : IRequest<BaseResponse<string>>
    {
        public string Email { get; set; } = default!;
    }

    public sealed class InitiatePasswordResetRequestHandler : IRequestHandler<InitiatePasswordResetRequest, BaseResponse<string>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<InitiatePasswordResetRequestHandler> _logger;
        private readonly INotificationService _notificationService;
        private IUtilityService _utilityService;
        public InitiatePasswordResetRequestHandler(AppDbContext context, ILogger<InitiatePasswordResetRequestHandler> logger, IUtilityService utilityService, INotificationService notification)
        {
            _context = context;
            _logger = logger;
            _utilityService = utilityService;
            _notificationService = notification;
        }
        public async ValueTask<BaseResponse<string>> Handle(InitiatePasswordResetRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var email = request.Email.ToLower().Trim();
                var user = await _context.AppUsers.Where(x => x.Email == email).Select(x => new { x.Id, x.Email }).FirstOrDefaultAsync(cancellationToken);
                if (user is null)
                    return new BaseResponse<string>(false, "The user tied to this email could not be found.");

                var otp = await _utilityService.GenerateOtpAsync(user.Id, cancellationToken);
                var emailResponse = await _notificationService.AdminSendOTPNotificationAsync(new MailRequest
                {
                    Email = user.Email!
                }, otp.Code, cancellationToken);

                if (!emailResponse.Status)
                    return new BaseResponse<string>(false, emailResponse.Message);

                return new BaseResponse<string>(true, "An OTP has been sent to your mail.");
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_InitiatePasswordResetRequest => Application ran into an error while trying to login admin user");
                return new BaseResponse<string>(false, "Application ran into an error.");
            }
        }
    }
}