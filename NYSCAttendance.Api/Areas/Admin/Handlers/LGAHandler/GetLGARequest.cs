using Mediator;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler
{
    public sealed record GetLGARequest : IRequest<BaseResponse<LGAResponse>>
    {
        internal long LGAId { get; set; }
    }

    public sealed class GetLGARequestHandler : IRequestHandler<GetLGARequest, BaseResponse<LGAResponse>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetLGARequestHandler> _logger;
        public GetLGARequestHandler(AppDbContext context, ILogger<GetLGARequestHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async ValueTask<BaseResponse<LGAResponse>> Handle(GetLGARequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.LGAId == 0)
                    return new BaseResponse<LGAResponse>(true, "LGA not found.");

                var lga = await _context.LGAs.Where(x => x.Id == request.LGAId).Select(x => new LGAResponse
                {
                    Id = x.Id,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    Name = x.Name,
                    DistanceInMeters = x.DistanceInMeters,
                    Token = x.Token
                }).FirstOrDefaultAsync(cancellationToken);

                if (lga is null)
                    return new BaseResponse<LGAResponse>(false, "LGA not found.");

                return new BaseResponse<LGAResponse>(true, "LGA retrieved successfully.", lga);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_GetLGARequestHandler => Application ran into an error while trying to retrieve LGA.");
                return new BaseResponse<LGAResponse>(false, "Application ran into an error");
            }
        }
    }
}