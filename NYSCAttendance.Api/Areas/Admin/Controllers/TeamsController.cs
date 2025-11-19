using System.Net;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NYSCAttendance.Api.Areas.Admin.Handlers.Auth;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Middleware;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.Admin.Controllers;

[Area("Admin")]
[ApiController]
[Route("api/[area]/[controller]")]
[Tags("Admin-Teams")]
[Authorize(Policy = AppConstants.AdminPolicyCode)]
public sealed class TeamsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    [AdminAuthorization(AppConstants.TeamManagement)]
    public async Task<IActionResult> AddTeamMember([FromBody] AddTeamMemberRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<PaginatedResponse<TeamMembersResponse>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    [AdminAuthorization(AppConstants.TeamManagement)]
    public async Task<IActionResult> GetTeamMembers([FromQuery] GetTeamMembersRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPatch("{id:long}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    [AdminAuthorization(AppConstants.TeamManagement)]
    public async Task<IActionResult> UpdateTeamMemberPermission([FromRoute] long id, [FromBody] UpdateTeamMemberPermissionRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    [AdminAuthorization(AppConstants.TeamManagement)]
    public async Task<IActionResult> RemoveTeamMember([FromRoute] long id, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new RemoveTeamMemeberRequest
        {
            Id = id
        }, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }
}