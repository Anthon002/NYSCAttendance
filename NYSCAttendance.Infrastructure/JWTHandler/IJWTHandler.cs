using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NYSCAttendance.Infrastructure.Data.Models;
using NYSCAttendance.Infrastructure.Utils;

namespace NYSCAttendance.Infrastructure.JWTHandler;


public sealed record JWTRequest
{
    public long Id { get; set; }
    public string Email { get; set; } = default!;
    public UserTypeEnum UserType { get; set; }
    public string[]? Permissions { get; set; }
    public string PolicyCode { get; set; } = default!;
}
public interface IJWTHandler
{
    LoginResponse Create(JWTRequest request);
}

public sealed record JWTRequestHandler : IJWTHandler
{
    private readonly AppSettingsOptions _options;
    private readonly JwtHeader _jwtHeader;
    private readonly SecurityKey _issuerSigningKey;
    private readonly SigningCredentials _signingCredentials;
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    public JWTRequestHandler(IOptionsSnapshot<AppSettingsOptions> options)
    {
        _options = options.Value;

        _issuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_options.JWTSettings!.Secret!));
        _signingCredentials = new SigningCredentials(_issuerSigningKey, SecurityAlgorithms.HmacSha256);
        _jwtHeader = new JwtHeader(_signingCredentials);
        _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }
    public LoginResponse Create(JWTRequest request)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var expires = nowUtc.AddDays(_options!.JWTSettings!.ExpiryTime);
        var payload = new JwtPayload
        {
            {"sub", request.Id},
            {"iss", _options!.JWTSettings!.Issuer},
            {"iat", nowUtc.ToUnixTimeSeconds()},
            {"exp", expires.ToUnixTimeSeconds()},
            {"unique_name", request.Email},
            {"Privileges", request.Permissions ?? []},
            {"PolicyCode", request.PolicyCode}
        };

        var jwt = new JwtSecurityToken(_jwtHeader, payload);
        var token = _jwtSecurityTokenHandler.WriteToken(jwt);

        return new LoginResponse
        {
            Token = token,
            ExpiresTime = expires,
            Email = request.Email,
            Id = request.Id
        };
    }
}
