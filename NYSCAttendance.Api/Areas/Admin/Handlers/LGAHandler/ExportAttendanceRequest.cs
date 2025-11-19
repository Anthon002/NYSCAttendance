using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Repos.Services.Contracts;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler
{
    public sealed record ExportAttendanceRequest : IRequest<BaseResponse<byte[]>>
    {
        internal long LGAId { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public BatchEnum? Batch { get; set; }
        public int? DayOfWeek { get; set; }
        public string? Search { get; set; }
    }

    public sealed class ExportAttendanceRequestHandler : IRequestHandler<ExportAttendanceRequest, BaseResponse<byte[]>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExportAttendanceRequestHandler> _logger;
        private readonly IUtilityService _utilityService;
        public ExportAttendanceRequestHandler(AppDbContext context, ILogger<ExportAttendanceRequestHandler> logger, IUtilityService utilityService)
        {
            _context = context;
            _logger = logger;
            _utilityService = utilityService;
        }
        public async ValueTask<BaseResponse<byte[]>> Handle(ExportAttendanceRequest request, CancellationToken cancellationToken)
        {
            try
            {
                DateTimeOffset? startTimeStamp = null;
                DateTimeOffset? endTimeStamp = null;

                if (request.From != null)
                    startTimeStamp = request.From.Value.Date;

                if (request.To != null)
                    endTimeStamp = request.To.Value.AddDays(1).AddTicks(-1);

                if (!await _context.LGAs.AnyAsync(x => x.Id == request.LGAId, cancellationToken))
                    return new BaseResponse<byte[]>(false, "Attendance location not found.");

                var search = request.Search?.Trim().ToLower();

                var records = await (from attendance in _context.Attendances.Select(x => new { x.FirstName, x.LastName, x.MiddleName, x.StateCode, x.LGAId, x.Batch, x.Day, x.CreatedAt })
                                     where attendance.LGAId == request.LGAId
                                       && (request.Batch == null || request.Batch == attendance.Batch)
                                       && (request.DayOfWeek == null || request.DayOfWeek == attendance.Day)
                                       && (startTimeStamp == null || attendance.CreatedAt >= startTimeStamp)
                                       && (endTimeStamp == null || attendance.CreatedAt <= endTimeStamp)
                                       && (search == null || attendance.FirstName.ToLower().Contains(search)
                                                          || attendance.LastName.ToLower().Contains(search)
                                                          || attendance.MiddleName.ToLower().Contains(search)
                                                          || attendance.StateCode.ToLower().Contains(search))
                                     select new AttendanceResponse
                                     {
                                         FirstName = attendance.FirstName,
                                         LastName = attendance.LastName,
                                         MiddleName = attendance.MiddleName,
                                         RecordedAt = attendance.CreatedAt,
                                         StateCode = attendance.StateCode,
                                         DayInt = attendance.Day
                                     }).ToArrayAsync(cancellationToken);

                var file = _utilityService.ExportAttenanceData(records);
                return new BaseResponse<byte[]>(true, "Records exported successfully.", file);
                        
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_ExportAttendanceRequestHandler => Application ran into an error while trying to retrieve attendance records.");
                return new BaseResponse<byte[]>(false, "Application ran into an error");
            }
        }
    }
}