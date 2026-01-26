using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace StockManagement.Api.Authentication;

public class MockJwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MockJwtAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.NoResult());

        var header = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
            return Task.FromResult(AuthenticateResult.NoResult());

        var token = header.Substring("Bearer ".Length).Trim();
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2)
                return Task.FromResult(AuthenticateResult.Fail("Invalid token format"));

            var payload = parts[1];
            // Base64url decode
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var claims = new List<Claim>();

            if (root.TryGetProperty("name", out var name)) claims.Add(new Claim(ClaimTypes.Name, name.GetString() ?? string.Empty));
            if (root.TryGetProperty("sub", out var sub)) claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString() ?? string.Empty));

            // roles may be a string or array
            if (root.TryGetProperty("role", out var roleProp))
            {
                if (roleProp.ValueKind == JsonValueKind.String)
                    claims.Add(new Claim(ClaimTypes.Role, roleProp.GetString() ?? string.Empty));
                else if (roleProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in roleProp.EnumerateArray())
                        claims.Add(new Claim(ClaimTypes.Role, r.GetString() ?? string.Empty));
                }
            }

            if (root.TryGetProperty("roles", out var rolesProp) && rolesProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in rolesProp.EnumerateArray())
                    claims.Add(new Claim(ClaimTypes.Role, r.GetString() ?? string.Empty));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "MockJwt decode failed");
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }
    }
}
