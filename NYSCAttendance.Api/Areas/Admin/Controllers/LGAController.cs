using System.Net;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NYSCAttendance.Api.Areas.Admin.Handlers.LGAHandler;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Middleware;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Api.Areas.Admin.Controllers;

[Area("Admin")]
[ApiController]
[Route("api/[area]/[controller]")]
[Tags("Admin-LGA")]
[Authorize(Policy = AppConstants.AdminPolicyCode)]
public sealed class LGAController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<PaginatedResponse<LGAResponse>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetLGAs([FromQuery] GetLGAsRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<LGAResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetLGA([FromRoute] long id, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetLGARequest
        {
            LGAId = id
        }, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("{id:long}/Attendance")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<PaginatedResponse<AttendanceResponse>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetAttendance([FromRoute] long id, [FromQuery] GetAttendanceRequest request, CancellationToken cancellationToken)
    {
        request.LGAId = id;
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("{id:long}/Export")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ExportAttendance([FromRoute] long id, [FromQuery] ExportAttendanceRequest request, CancellationToken cancellationToken)
    {
        request.LGAId = id;
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return File(response.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"attendance-record"); ;
    }

    [HttpPost("{id:long}/Reserves")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    [AdminAuthorization(AppConstants.LGAManagement)]
    public async Task<IActionResult> ReserveSpot([FromRoute] long id, [FromQuery] ReserveSpotRequest request, CancellationToken cancellationToken)
    {
        request.LGAId = id;
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    [AdminAuthorization(AppConstants.LGAManagement)]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPatch("{id:long}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    [AdminAuthorization(AppConstants.LGAManagement)]
    public async Task<IActionResult> UpdateLocation([FromRoute] long id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }
}