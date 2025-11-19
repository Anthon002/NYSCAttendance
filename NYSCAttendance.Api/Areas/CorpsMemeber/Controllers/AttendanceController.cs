using System.Net;
using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NYSCAttendance.Api.Areas.CorpsMemeber.Handlers.Attendances;
using NYSCAttendance.Infrastructure.Data.Entities;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Api.Areas.CorpsMemeber.Controllers;

[Area("CorpsMember")]
[ApiController]
[Route("api/[area]/[controller]")]
[Tags("CorpsMemebers-Attendance")]
public sealed class AttendanceController(ISender sender, IPasswordHasher<AppUser> passwordHasher) : ControllerBase
{
    private readonly ISender _sender = sender;
    private readonly IPasswordHasher<AppUser> _passwordHasher = passwordHasher;

    [HttpPost("{token}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<long>), ((int)HttpStatusCode.OK))]
    [ProducesResponseType(typeof(BaseResponse<long>), ((int)HttpStatusCode.BadRequest))]
    [EnableRateLimiting("policy1")]
    public async Task<IActionResult> RecordAttendance([FromRoute] string token, [FromBody] RecordAttendanceRequest request, CancellationToken cancellationToken)
    {
        request.Token = token;
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpGet("{identifier}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<long>), ((int)HttpStatusCode.OK))]
    [ProducesResponseType(typeof(BaseResponse<long>), ((int)HttpStatusCode.BadRequest))]
    [EnableRateLimiting("policy1")]
    public async Task<IActionResult> GetAttendanceRecord([FromRoute] string identifier, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAttendanceRecordRequest
        {
            Identifier = identifier
        }, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }
}