using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler
{
    public sealed record GetAttendanceRequest : IRequest<BaseResponse<PaginatedResponse<AttendanceResponse>>>
    {
        internal long LGAId { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public BatchEnum? Batch { get; set; }
        public CDSEnum? CDS { get; set; }
        public int? DayOfWeek { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public sealed class GetAttendanceRequestHandler : IRequestHandler<GetAttendanceRequest, BaseResponse<PaginatedResponse<AttendanceResponse>>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetAttendanceRequestHandler> _logger;
        public GetAttendanceRequestHandler(AppDbContext context, ILogger<GetAttendanceRequestHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async ValueTask<BaseResponse<PaginatedResponse<AttendanceResponse>>> Handle(GetAttendanceRequest request, CancellationToken cancellationToken)
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
                    return new BaseResponse<PaginatedResponse<AttendanceResponse>>(false, "Attendance location not found.");

                var search = request.Search?.Trim().ToLower();

                var iAttendance = from attendance in _context.Attendances.Select(x => new { x.FirstName, x.LastName, x.MiddleName, x.StateCode, x.LGAId, x.Batch, x.Day, x.CreatedAt, x.CDS })
                                  where attendance.LGAId == request.LGAId
                                    && (request.Batch == null || request.Batch == attendance.Batch)
                                    && (request.DayOfWeek == null || request.DayOfWeek == attendance.Day)
                                    && (startTimeStamp == null || attendance.CreatedAt >= startTimeStamp)
                                    && (endTimeStamp == null || attendance.CreatedAt <= endTimeStamp)
                                    && (request.CDS == null || attendance.CDS == request.CDS)
                                    && (search == null || attendance.FirstName.ToLower().Contains(search)
                                                       || attendance.LastName.ToLower().Contains(search)
                                                       || attendance.MiddleName.ToLower().Contains(search))
                                  select new AttendanceResponse
                                  {
                                      FirstName = attendance.FirstName,
                                      LastName = attendance.LastName,
                                      MiddleName = attendance.MiddleName,
                                      RecordedAt = attendance.CreatedAt,
                                      StateCode = attendance.StateCode,
                                      DayInt = attendance.Day
                                  };
                var totalRecordsCount = await iAttendance.CountAsync(cancellationToken);
                var records = await iAttendance.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToArrayAsync(cancellationToken);
                records = records.Select(x =>
                {
                    x.Day = ((DayOfWeek)x.DayInt).ToString();
                    return x;
                }).ToArray();

                return new BaseResponse<PaginatedResponse<AttendanceResponse>>(true, "Attendance records retrieved successfully.", new PaginatedResponse<AttendanceResponse>(records, totalRecordsCount, request.PageNumber, request.PageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_GetAttendanceRequestHandler => Application ran into an error while trying to retrieve attendance records.");
                return new BaseResponse<PaginatedResponse<AttendanceResponse>>(false, "Application ran into an error");
            }
        }
    }
}