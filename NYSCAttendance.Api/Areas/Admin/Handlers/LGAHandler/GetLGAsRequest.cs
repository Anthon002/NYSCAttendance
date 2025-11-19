using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler
{
    public sealed record GetLGAsRequest : IRequest<BaseResponse<PaginatedResponse<LGAResponse>>>
    {
        public string? Search { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    public sealed class GetLGAsRequestHandler : IRequestHandler<GetLGAsRequest, BaseResponse<PaginatedResponse<LGAResponse>>>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetLGAsRequestHandler> _logger;
        public GetLGAsRequestHandler(AppDbContext context, ILogger<GetLGAsRequestHandler> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async ValueTask<BaseResponse<PaginatedResponse<LGAResponse>>> Handle(GetLGAsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var search = request.Search?.ToLower().Trim();
                var iLGA = from lga in _context.LGAs.Select(x => new { x.Name, x.Latitude, x.Longitude, x.DistanceInMeters, x.Id, x.Token })
                           where (search == null || lga.Name.ToLower().Trim().Contains(search))
                           orderby lga.Name ascending
                           select new LGAResponse
                           {
                               Name = lga.Name,
                               Latitude = lga.Latitude,
                               Longitude = lga.Longitude,
                               DistanceInMeters =lga.DistanceInMeters,
                               Id = lga.Id,
                               Token = lga.Token
                           };
                var totalRecordsCount = await iLGA.CountAsync(cancellationToken);
                var records = await iLGA.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToArrayAsync(cancellationToken);
                return new BaseResponse<PaginatedResponse<LGAResponse>>(true, "LGAs returned successfully.", new PaginatedResponse<LGAResponse>(records, totalRecordsCount, request.PageNumber, request.PageSize));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin_GetLGAsRequestHandler => Application ran into an error while trying to retrieve LGAs");
                return new BaseResponse<PaginatedResponse<LGAResponse>>(false, "Application ran into an error");
            }
        }
    }
}