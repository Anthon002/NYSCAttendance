using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.Auth;

public sealed record LoginRequest : IRequest<BaseResponse<string>>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public sealed class LoginRequestHandler : IRequestHandler<LoginRequest, BaseResponse<string>>
{
    private readonly AppDbContext _context;
    private readonly ILogger<LoginRequestHandler> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUtilityService _utilityService;
    private readonly INotificationService _notificationService;
    public LoginRequestHandler(AppDbContext context, ILogger<LoginRequestHandler> logger, IUtilityService utilityService, UserManager<AppUser> userManager, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _utilityService = utilityService;
        _userManager = userManager;
        _notificationService = notificationService;
    }
    public async ValueTask<BaseResponse<string>> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var email = request.Email.ToLower().Trim();
            var user = await (from usr in _context.AppUsers
                              where usr.Email == email
                              select new
                              {
                                  usr
                              }).FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                return new BaseResponse<string>(false, "A user tied to this email could not be found.");

            // validate password
            OTPResponse otp;
            var passwordValidation = await _userManager.CheckPasswordAsync(user.usr, request.Password);
            if (passwordValidation)
                otp = await _utilityService.GenerateOtpAsync(user.usr.Id, cancellationToken);
            else
                return new BaseResponse<string>(false, "Password is incorrect.");

            await _notificationService.AdminSendOTPNotificationAsync(new MailRequest
            {
                Email = user.usr.Email!,
                FirstName = user.usr.FirstName
            }, otp.Code, cancellationToken);

            return new BaseResponse<string>(true, "Admin login initated successfully. An OTP has been sent to your email.", otp.Identifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin_LoginRequest => Application ran into an error while trying to login admin user");
            return new BaseResponse<string>(false, "Application ran into an error.");
        }
    }
}