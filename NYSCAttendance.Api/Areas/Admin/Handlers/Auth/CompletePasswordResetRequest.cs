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
    public sealed record CompletePasswordResetRequest : IRequest<BaseResponse>
    {
        public string Identifier { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }

    public sealed class CompletePasswordResetRequestHandler : IRequestHandler<CompletePasswordResetRequest, BaseResponse>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CompletePasswordResetRequestHandler> _logger;
        private IUtilityService _utilityService;
        private IPasswordHasher<AppUser> _passwordHasher;
        public CompletePasswordResetRequestHandler(AppDbContext context, ILogger<CompletePasswordResetRequestHandler> logger, IUtilityService utilityService, IPasswordHasher<AppUser> passwordHasher)
        {
            _context = context;
            _logger = logger;
            _utilityService = utilityService;
            _passwordHasher = passwordHasher;
        }
        public async ValueTask<BaseResponse> Handle(CompletePasswordResetRequest request, CancellationToken cancellationToken)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    var otpResponse = await _utilityService.CompleteOtpAsync(request.Identifier, request.Code, cancellationToken);
                    if (otpResponse.Status)
                        return new BaseResponse(false, otpResponse.Message);

                    var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Id == otpResponse.Value, cancellationToken);
                    if (user is null)
                        return new BaseResponse(false, "User not found.");

                    if (request.Password != request.ConfirmPassword)
                        return new BaseResponse(false, "Passwords do not match. Please check and try again.");

                    var passwordHashed = _passwordHasher.HashPassword(user, request.Password);
                    user.PasswordHash = passwordHashed;
                    user.UpdatedAt = DateTimeOffset.UtcNow;

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new BaseResponse(true, "Password reset successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Admin_CompletePasswordResetRequest => Application ran into an error while trying to complete password reset.");
                    return new BaseResponse(false, "Application ran into an error.");
                }
            }
        }
    }
}