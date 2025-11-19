using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.JWTHandler;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.Auth;

public sealed record ConfirmLoginRequest : IRequest<BaseResponse<LoginResponse>>
{
    public string Identifier { get; set; } = default!;
    public string Code { get; set; } = default!;
}

public sealed class ConfirmLoginRequestHandler : IRequestHandler<ConfirmLoginRequest, BaseResponse<LoginResponse>>
{
    private readonly AppDbContext _context;
    private readonly ILogger<ConfirmLoginRequestHandler> _logger;
    private IUtilityService _utilityService;
    private readonly IJWTHandler _jwtHandler;
    private readonly SignInManager<AppUser> _signInManager;
    public ConfirmLoginRequestHandler(AppDbContext context, ILogger<ConfirmLoginRequestHandler> logger, IUtilityService utilityService, IJWTHandler jwtHandler, SignInManager<AppUser> signInManager)
    {
        _context = context;
        _logger = logger;
        _utilityService = utilityService;
        _jwtHandler = jwtHandler;
        _signInManager = signInManager;
    }
    public async ValueTask<BaseResponse<LoginResponse>> Handle(ConfirmLoginRequest request, CancellationToken cancellationToken)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
        {
            try
            {
                var otpResponse = await _utilityService.CompleteOtpAsync(request.Identifier, request.Code, cancellationToken);
                if (!otpResponse.Status)
                    return new BaseResponse<LoginResponse>(false, otpResponse.Message);

                var user = await (from usr in _context.AppUsers.Where(x => x.Id == otpResponse.Value)
                                  select new
                                  {
                                      usr,
                                      permissions = _context.AppUserClaims.Where(x => x.UserId == usr.Id).Select(x => x.ClaimValue == null ? default : x.ClaimType!.ToString()).ToArray()
                                  }).FirstOrDefaultAsync(cancellationToken);
                if (user is null)
                    return new BaseResponse<LoginResponse>(false, "User not found. Try logging in again.");

                await _signInManager.SignInAsync(user.usr, true);

                var loginResponse = _jwtHandler.Create(new JWTRequest
                {
                    Email = user.usr.Email!,
                    Id = user.usr.Id,
                    Permissions = user.permissions,
                    PolicyCode = AppConstants.AdminPolicyCode
                });

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new BaseResponse<LoginResponse>(true, "Login successful.", loginResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Admin_ConfirmLoginRequest => Application ran into an error while trying to login admin.");
                return new BaseResponse<LoginResponse>(false, "Application ran into an error.");
            }
        }
    }
}