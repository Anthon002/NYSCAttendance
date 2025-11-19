using Mediator;
using Microsoft.EntityFrameworkCore;
using NYSCAttendance.Infrastructure.Data;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Api.Areas.Admin.Handlers.Auth;

public sealed record GetTeamMembersRequest : IRequest<BaseResponse<PaginatedResponse<TeamMembersResponse>>>
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public sealed class GetTeamMembersRequestHandler : IRequestHandler<GetTeamMembersRequest, BaseResponse<PaginatedResponse<TeamMembersResponse>>>
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetTeamMembersRequestHandler> _logger;
    public GetTeamMembersRequestHandler(AppDbContext context, ILogger<GetTeamMembersRequestHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async ValueTask<BaseResponse<PaginatedResponse<TeamMembersResponse>>> Handle(GetTeamMembersRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var search = request.Search?.Trim().ToLower();
            var iUsers = from user in _context.AppUsers
                         where (search == null || user.FirstName.ToLower().Contains(search) || user.LastName.ToLower().Contains(search) || user.Email!.ToLower().Contains(search))
                         select new TeamMembersResponse
                         {
                             Email = user.Email!,
                             FirstName = user.FirstName,
                             Id = user.Id,
                             LastName = user.LastName,
                             Permissions = _context.AppUserClaims.Where(x => x.UserId == user.Id).Select(x => x.ClaimValue).ToArray()!,
                             UserType = user.UserType
                         };

            var totalRecordsCount = await iUsers.CountAsync(cancellationToken);
            var records = await iUsers.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToArrayAsync(cancellationToken);

            return new BaseResponse<PaginatedResponse<TeamMembersResponse>>(true, "Admins returned successfully.", new PaginatedResponse<TeamMembersResponse>(records, totalRecordsCount, request.PageNumber, request.PageSize));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin_GetTeamMembersRequest => Application ran into an error while trying to get team members");
            return new BaseResponse<PaginatedResponse<TeamMembersResponse>>(false, "Application ran into an error.");
        }
    }
}