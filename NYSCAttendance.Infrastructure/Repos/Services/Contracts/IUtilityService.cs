using System;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Infrastructure.Repos.Services.Contracts;

public interface IUtilityService
{
    Task<OTPResponse> GenerateOtpAsync(long userid, CancellationToken cancellationToken);
    Task<BaseResponse<long>> CompleteOtpAsync(string Identifier, string otp, CancellationToken cancellationToken);
    string GeneratePassword(int length);

    byte[] ExportAttenanceData(IEnumerable<AttendanceResponse> source);
}