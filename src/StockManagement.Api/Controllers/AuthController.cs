using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace StockManagement.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    [HttpPost("token")]
    public IActionResult Token([FromForm] string username, [FromForm] string role)
    {
        // Allow mock unsigned tokens in development by setting USE_MOCK_JWT (or default true)
        var useMock = (Environment.GetEnvironmentVariable("USE_MOCK_JWT") ?? "true").ToLowerInvariant() != "false";

        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "stockmanagement";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "stockmanagement_audience";

        if (useMock)
        {
            // Produce an unsigned token (alg=none) compatible with the mock JWT handler
            var header = JsonSerializer.Serialize(new { alg = "none", typ = "JWT" });
            var payloadObj = new Dictionary<string, object>
            {
                ["name"] = username,
                ["sub"] = username,
                ["role"] = role,
                ["iss"] = issuer,
                ["aud"] = audience,
                ["exp"] = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds()
            };
            var payload = JsonSerializer.Serialize(payloadObj);

            string Base64UrlEncode(string str)
            {
                var bytes = Encoding.UTF8.GetBytes(str);
                var base64 = Convert.ToBase64String(bytes);
                return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
            }

            var unsignedToken = $"{Base64UrlEncode(header)}.{Base64UrlEncode(payload)}.";
            return Ok(new { token = unsignedToken, mock = true });
        }

        // Strict signing path: use configured secret or a long default for development
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_super_secret_long_default_for_development_2026!!";
        if (string.IsNullOrEmpty(jwtSecret) || Encoding.UTF8.GetBytes(jwtSecret).Length < 32)
        {
            return BadRequest(new { error = "JWT_SECRET must be set and at least 32 bytes for HS256 signing. Use USE_MOCK_JWT=true for development mock tokens." });
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), mock = false });
    }
}
