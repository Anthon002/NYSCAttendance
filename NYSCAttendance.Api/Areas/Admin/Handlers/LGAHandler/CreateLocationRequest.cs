using Mediator;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Data.Entities;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler;

public sealed record CreateLocationRequest : IRequest<BaseResponse<string>>
{
    public string Name { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceInMeters { get; set; }
    public string OpensAt { get; set; } = default!;
    public string ClosesAt { get; set; } = default!;
}

public sealed class CreateLocationRequestHandler : IRequestHandler<CreateLocationRequest, BaseResponse<string>>
{
    private readonly AppDbContext _context;
    private readonly ILogger<CreateLocationRequestHandler> _logger;
    public CreateLocationRequestHandler(AppDbContext context, ILogger<CreateLocationRequestHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async ValueTask<BaseResponse<string>> Handle(CreateLocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _context.LGAs.AnyAsync(x => x.Name.ToLower() == request.Name.ToLower().Trim()))
                return new BaseResponse<string>(false, "A location under this LGA has already been created.");

            if (request.Latitude < -90.0 || request.Latitude > 90.0)
                return new BaseResponse<string>(false, "Invalid location. Please check and try again.");

            // Validate longitude
            if (request.Longitude < -180.0 || request.Longitude > 180.0)
                return new BaseResponse<string>(false, "Invalid location. Please check and try again.");

           // validating opening hour
            if (int.TryParse(request.OpensAt.Split(":")[0], out int openHr))
            {
                if (openHr < 0 || openHr > 23)
                    return new BaseResponse<string>(false, "Invalid opening hour provided. Please check and try again.");
            }
            else
                openHr = 7;

            //validate opening minute
            if (int.TryParse(request.OpensAt.Split(":")[1], out int openMin))
            {
                if (openMin < 0 || openMin > 59)
                    return new BaseResponse<string>(false, "Invalid opening minute provided. Please check and try again.");
            }
            else
                openMin = 0;

            //validate closing hour
            if (int.TryParse(request.ClosesAt.Split(":")[0], out int closeHr))
            {
                if (closeHr < 0 || closeHr > 23)
                    return new BaseResponse<string>(false, "Invalid opening hour provided. Please check and try again.");
            }
            else
                closeHr = 9;

            //validate closing minute
            if (int.TryParse(request.ClosesAt.Split(":")[1], out int closeMin))
            {
                if (closeMin < 0 || closeMin > 59)
                    return new BaseResponse<string>(false, "Invalid opening minute provided. Please check and try again.");
            }
            else
                closeMin = 0;

            using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    var token = Guid.NewGuid().ToString().Replace("-","").Substring(0,10);
                    await _context.LGAs.AddAsync(new LGA
                    {
                        CreatedAt = DateTimeOffset.UtcNow,
                        DistanceInMeters = request.DistanceInMeters,
                        Latitude = request.Latitude,
                        Longitude = request.Longitude,
                        Token = token!,
                        Name = request.Name,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        OpenTime = request.OpensAt,
                        CloseTime = request.ClosesAt
                    }, cancellationToken);

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return new BaseResponse<string>(true, "Attendance point created successfully.", token);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Admin_CreateLocationRequestHandler => Application ran into an error while trying to create LGA attendance point.");
                    return new BaseResponse<string>(false, "Application ran into an error");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin_CreateLocationRequestHandler => Application ran into an error while trying to to create LGA attendance point.");
            return new BaseResponse<string>(false, "Application ran into an error");
        }
    }
}