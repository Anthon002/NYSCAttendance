using Mediator;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler;

public sealed record UpdateLocationRequest : IRequest<BaseResponse>
{
    internal long Id { get; set; }
    public double DistanceInMeters { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = default!;
}

public sealed class UpdateLocationRequestHandler : IRequestHandler<UpdateLocationRequest, BaseResponse>
{
    private readonly AppDbContext _context;
    private readonly ILogger<UpdateLocationRequestHandler> _logger;
    public UpdateLocationRequestHandler(AppDbContext context, ILogger<UpdateLocationRequestHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async ValueTask<BaseResponse> Handle(UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _context.LGAs.AnyAsync(x => x.Id == request.Id, cancellationToken))
                return new BaseResponse(false, "Attendance location not found.");

            if (request.Latitude < -90.0 || request.Latitude > 90.0)
                return new BaseResponse(false, "Invalid location. Please check and try again.");

            // Validate longitude
            if (request.Longitude < -180.0 || request.Longitude > 180.0)
                return new BaseResponse(false, "Invalid location. Please check and try again.");

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    await _context.LGAs.ExecuteUpdateAsync(s => s
                        .SetProperty(c => c.DistanceInMeters, request.DistanceInMeters)
                        .SetProperty(c => c.Latitude, request.Latitude)
                        .SetProperty(c => c.Longitude, request.Longitude)
                        .SetProperty(c => c.Name, request.Name)
                        .SetProperty(c => c.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    return new BaseResponse(true, "Attendance point updated successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Admin_UpdateLocationRequestHandler => Application ran into an error while trying to update LGA attendance point.");
                    return new BaseResponse(false, "Application ran into an error");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin_UpdateLocationRequestHandler => Application ran into an error while trying to to update LGA attendance point.");
            return new BaseResponse(false, "Application ran into an error");
        }
    }
}