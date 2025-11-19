using System;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Infrastructure.Repos.Integrations.Contracts;

public interface IBrevo
{
    Task<BaseResponse> SendEmail(BrevoRequest request, CancellationToken cancellationToken);
}
