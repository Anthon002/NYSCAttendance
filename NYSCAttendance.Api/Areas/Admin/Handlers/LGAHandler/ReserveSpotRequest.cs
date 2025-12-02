using Mediator;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler
{
    public sealed record ReserveSpotRequest : IRequest<BaseResponse<long>>
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? StateCode { get; set; }
        public CDSEnum CDS { get; set; }
        public BatchEnum Batch { get; set; } = default;
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        internal long LGAId { get; set; }
    }

    public sealed class ReserveSpotRequestHandler : IRequestHandler<ReserveSpotRequest, BaseResponse<long>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReserveSpotRequestHandler> _logger;
        public ReserveSpotRequestHandler(AppDbContext context, ILogger<ReserveSpotRequestHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async ValueTask<BaseResponse<long>> Handle(ReserveSpotRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.LGAId <= 0)
                    return new BaseResponse<long>(false, "LGA not found.");

                // Validate latitude
                if (request.Latitude < -90 || request.Latitude > 90)
                    return new BaseResponse<long>(false, "Invalid latitude. Latitude must be between -90 and 90.");
                // Validate longitude
                if (request.Longitude < -180 || request.Longitude > 180)
                    return new BaseResponse<long>(false, "Invalid longitude. Longitude must be between -180 and 180.");

                if (request.FirstName?.Length > 100)
                    return new BaseResponse<long>(false, "First name has exceeded our character limit.");

                if (request.LastName?.Length > 100)
                    return new BaseResponse<long>(false, "Last name has exceeded our character limit.");

                if (request.MiddleName?.Length > 100)
                    return new BaseResponse<long>(false, "Middle name has exceeded our character limit.");

                if (request.StateCode?.Length > 20)
                    return new BaseResponse<long>(false, "State code has exceeded our character limit.");

                if (!await _context.LGAs.AnyAsync(x => x.Id == request.LGAId, cancellationToken))
                    return new BaseResponse<long>(false, "LGA not found.");

                using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        var now = DateTimeOffset.UtcNow;
                        var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                        var previousAttendance = await _context.Attendances.Where(x => x.LGAId == request.LGAId && x.CreatedAt >= today).Select(x => new { x.SerialNumber, x.CreatedAt }).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

                        long serialNumber = 0;
                        if (previousAttendance is not null)
                            serialNumber = previousAttendance.SerialNumber;

                        var attendance = new Attendance
                        {
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            MiddleName = request.MiddleName,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            Identifier = "",
                            SerialNumber = serialNumber + 1,
                            Batch = request.Batch,
                            LGAId = request.LGAId,
                            Day = (int)now.DayOfWeek,
                            StateCode = request.StateCode,
                            CDS = request.CDS,
                            ISReserve = true
                        };

                        await _context.Attendances.AddAsync(attendance, cancellationToken);
                        await _context.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);

                        return new BaseResponse<long>(true, "Attendance record saved.", attendance.SerialNumber);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogError(ex, "Admin_ReserveSpotRequestHandler => Application ran into an error while trying to retrieve LGA.");
                        return new BaseResponse<long>(false, "Application ran into an error");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_ReserveSpotRequestHandler => Application ran into an error while trying to retrieve LGA.");
                return new BaseResponse<long>(false, "Application ran into an error");
            }
        }
    }
}