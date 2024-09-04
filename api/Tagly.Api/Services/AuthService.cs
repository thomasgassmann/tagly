using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Grpc.Core;
using Microsoft.IdentityModel.Tokens;

namespace Tagly.Api.Services;

public class AuthService : Auth.AuthBase
{
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(ILogger<AuthService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override Task<AuthReply> Login(AuthRequest request, ServerCallContext context)
    {
        if (request.Secret != _configuration["Secret"])
        {
            _logger.LogInformation("Could not authenticate for user {0}", request.Secret);
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Not authenticated"));
        }

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["User"]!),
                new Claim(JwtRegisteredClaimNames.Email, _configuration["User"]!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);
        return Task.FromResult(new AuthReply
        {
            JwtToken = jwtToken
        });
    }
}
