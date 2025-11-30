using Mediator;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.CorpsMemeber.Handlers.Attendances;

public record RecordAttendanceRequest : IRequest<BaseResponse<long>>
{
    public string Identifier { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string StateCode { get; set; } = default!;
    public BatchEnum Batch { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    internal string Token { get; set; } = default!;
    public CDSEnum CDS { get; set; }
}

public sealed class RecordAttendanceRequestHandler : IRequestHandler<RecordAttendanceRequest, BaseResponse<long>>
{
    private readonly ILogger<RecordAttendanceRequestHandler> _logger;
    private readonly AppDbContext _context;
    public RecordAttendanceRequestHandler(ILogger<RecordAttendanceRequestHandler> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async ValueTask<BaseResponse<long>> Handle(RecordAttendanceRequest request, CancellationToken cancellationToken)
    {
        try
        {
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

            var att = await _context.Attendances.Where(x => x.Identifier == request.Identifier).Select(x => new { x.SerialNumber }).FirstOrDefaultAsync(cancellationToken);

            if (att is not null)
                return new BaseResponse<long>(true, "Attendance already recorded.", att.SerialNumber);

            var lga = await _context.LGAs.Where(x => x.Token.Trim() == request.Token).Select(x => new { x.Longitude, x.Latitude, x.DistanceInMeters, x.Id, x.Name }).FirstOrDefaultAsync(cancellationToken);
            if (lga is null)
                return new BaseResponse<long>(false, "LGA not found.");

            var userLocation = new Point(request.Longitude, request.Latitude);
            var designatedLocation = new Point(lga.Longitude, lga.Latitude);

            var distance = userLocation.Distance(designatedLocation) * 111 * 1000;

            if (distance > lga.DistanceInMeters)
                return new BaseResponse<long>(false, $"You are too far from the designated spot ({distance.ToString("F2")}m). Please get closer to {lga.Name} and try again.");

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    var now = (DateTimeOffset)DateTimeOffset.UtcNow.Date;
                    var today = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

                    var previousAttendance = await _context.Attendances.Where(x => x.LGAId == lga.Id && x.CreatedAt >= today).Select(x => new { x.SerialNumber, x.CreatedAt }).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync(cancellationToken);

                    long serialNumber = 1;
                    if (previousAttendance is not null)
                        serialNumber = previousAttendance.SerialNumber + 1;

                    var attendance = new Attendance
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        MiddleName = request.MiddleName,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        Identifier = request.Identifier.Trim(),
                        SerialNumber = serialNumber,
                        Batch = request.Batch,
                        LGAId = lga.Id,
                        Day = (int)now.DayOfWeek,
                        StateCode = request.StateCode,
                        CDS = request.CDS
                    };

                    await _context.Attendances.AddAsync(attendance, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return new BaseResponse<long>(true, "Attendance record saved.", attendance.SerialNumber);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "CorpsMember_RecordAttendanceRequest => Application ran into an error while trying to record corps memeber's attendance.");
                    return new BaseResponse<long>(false, "Application ran into an error.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CorpsMember_RecordAttendanceRequest => Application ran into an error while trying to record corps memeber's attendance.");
            return new BaseResponse<long>(false, "Application ran into an error.");
        }
    }
}