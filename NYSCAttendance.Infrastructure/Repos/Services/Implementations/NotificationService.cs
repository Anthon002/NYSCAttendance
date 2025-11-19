using Microsoft.Extensions.Options;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Repos.Integrations.Contracts;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.Repos.Services.Implementations;

public sealed record NotificationService : INotificationService
{
    private readonly IBrevo _emailService;
    private readonly AppSettingsOptions _options;
    public NotificationService(IBrevo emailService, IOptionsSnapshot<AppSettingsOptions> options)
    {
        _emailService = emailService;
        _options = options.Value;
    }

    public async Task<BaseResponse> AdminSendAccountRemovedNotificationAsync(MailRequest request, CancellationToken cancellationToken)
    {
        var message = $"Hi {request.FirstName},<br><br>Your account has been removed.<br><br>You no longer have access to the dashboard and can not longer manage corp memebers attendance records. <br>If this was a mistake, please reach out to an admin with the proper permission.";

        var response = await _emailService.SendEmail(new BrevoRequest(message, $"{_options.AppSettings?.AppName} | Account Removed.", request.FirstName, request.Email), cancellationToken);

        return new BaseResponse(response.Status, response.Message);
    }

    public async Task<BaseResponse> AdminSendLoginCredentialsNotificationAsync(MailRequest request, string password, CancellationToken cancellationToken)
    {
        var loginUrl = _options.AppSettings?.AdminUrl;
        var message = $"Hi {request.FirstName},<br><br>You have been invited as an admin on the NYSC attendance coordination application.<br><br>Here are your login credentials:<br>Email: <b>{request.Email}</b><br>Password: <b>{password}</b><br><br>Click the link below to login:<br><a href=\"{loginUrl}\">Log in</a>";

        var response = await _emailService.SendEmail(new BrevoRequest(message, $"{_options.AppSettings?.AppName} | Admin Invitation.", request.FirstName, request.Email), cancellationToken);

        return new BaseResponse(response.Status, response.Message);
    }

    public async Task<BaseResponse> AdminSendOTPNotificationAsync(MailRequest request, string otp, CancellationToken cancellationToken)
    {
        var message = $"Hi {request.FirstName},<br><br>Your One-Time Password (OTP) is <b>{otp}</b>.<br><br>Please use this code to complete your verification. It will expire shortly.";

        var response = await _emailService.SendEmail(new BrevoRequest(message, $"{_options.AppSettings?.AppName} | Password Reset Initiated", request.FirstName, request.Email), cancellationToken);

        return new BaseResponse(response.Status, response.Message);
    }
}
