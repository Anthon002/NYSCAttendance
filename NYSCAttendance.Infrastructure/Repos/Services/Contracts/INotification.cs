using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Infrastructure.Repos.Services.Contracts;

public interface INotificationService
{
    Task<BaseResponse> AdminSendOTPNotificationAsync(MailRequest request, string otp, CancellationToken cancellationToken);
    Task<BaseResponse> AdminSendLoginCredentialsNotificationAsync(MailRequest request, string password, CancellationToken cancellationToken);
    Task<BaseResponse> AdminSendAccountRemovedNotificationAsync(MailRequest request, CancellationToken cancellationToken);
}
