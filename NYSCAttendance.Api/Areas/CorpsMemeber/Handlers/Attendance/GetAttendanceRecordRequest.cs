using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Api.Areas.CorpsMemeber.Handlers.Attendances;

public sealed record GetAttendanceRecordRequest : IRequest<BaseResponse<long>>
{
    internal string Identifier { get; set; } = default!;
}

public sealed class GetAttendanceRecordRequestHandler : IRequestHandler<GetAttendanceRecordRequest, BaseResponse<long>>
{
    private readonly ILogger<GetAttendanceRecordRequestHandler> _logger;
    private readonly AppDbContext _context;
    public GetAttendanceRecordRequestHandler(ILogger<GetAttendanceRecordRequestHandler> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async ValueTask<BaseResponse<long>> Handle(GetAttendanceRecordRequest request, CancellationToken cancellationToken)
    {
        try
        {

            var attendance = await _context.Attendances.Where(x => x.Identifier == request.Identifier.Trim()).Select(x => new { x.SerialNumber, x.CreatedAt }).FirstOrDefaultAsync(cancellationToken);

            if (attendance is null)
                return new BaseResponse<long>(false, "Your attendance has not been recorded. Please log your attendance to continue.");
            
            if (attendance.CreatedAt.Date != DateTimeOffset.UtcNow.Date)
                return new BaseResponse<long>(false, "Your attendance has not been recorded. Please log your attendance to continue.");
                
            return new BaseResponse<long>(true, "Attendance retrieved successfully.", attendance.SerialNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorpsMember_GetAttendanceRecordRequest => Application ran into an error while trying to record corps memeber's attendance.");
            return new BaseResponse<long>(false, "Application ran into an error.");
        }
    }
}