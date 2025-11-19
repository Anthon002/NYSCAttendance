using System.Net;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using NYSCAttendance.Api.Areas.Admin.Handlers.Auth;
using NYSCAttendance.Infrastructure.Data.Models;

namespace NYSCAttendance.Api.Areas.Admin.Controllers;

[Area("Admin")]
[ApiController]
[Route("api/[area]/[controller]")]
[Tags("Admin-Auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost("Login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("ConfirmLoginOTP")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<LoginResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ConfirmLogin([FromBody] ConfirmLoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("InitiatePasswordReset")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> InitiatePasswordReset([FromBody] InitiatePasswordResetRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("CompletePasswordReset")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(BaseResponse<string>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(BaseResponse), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CompletePasswordReset([FromBody] CompletePasswordResetRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);
        return Ok(response);
    }
}
