using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace StockManagement.Integration.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private string CreateJwt(string username, string role)
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_super_secret_change_me";
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "stockmanagement";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "stockmanagement_audience";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, role) };
        var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task Inputter_Can_Create_ControlledItem_And_Reconciler_Can_Generate_Report_Authoriser_Can_SignOff()
    {
        var client = _factory.CreateClient();

        // create item as inputter
        var tokenInputter = CreateJwt("user1", "Inputter");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInputter);

        var item = new {
            Type = "CreditCard",
            DateReceived = DateTime.UtcNow,
            ReceivedFrom = "HO",
            AccountNumber = "123",
            AccountName = "Test",
            ControlledNumber = "CC-999",
            ShortId = "S1",
            GhanaCardNumber = "",
            NameOnGhanaCard = "",
            RetentionDays = 30
        };

        var resp = await client.PostAsJsonAsync("/api/ControlledItems", item);
        resp.EnsureSuccessStatusCode();

        // set opening balance and transactions as inputter
        var ob = new { BranchId = "branchA", Type = "CreditCard", Date = DateTime.UtcNow.Date, Quantity = 1 };
        var obResp = await client.PostAsJsonAsync("/api/Transactions/opening", ob);
        obResp.EnsureSuccessStatusCode();

        // Generate report as reconciler
        var tokenReconciler = CreateJwt("recon1", "Reconciler");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenReconciler);
        var genResp = await client.PostAsync($"/api/Reports/eod/generate?branchId=branchA&date={DateTime.UtcNow.Date:yyyy-MM-dd}&generatedBy=recon1", null);
        genResp.EnsureSuccessStatusCode();
        var genObj = await genResp.Content.ReadFromJsonAsync<dynamic>();
        string reportId = (string)genObj.id;

        // Authoriser signs off
        var tokenAuthoriser = CreateJwt("auth1", "Authoriser");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAuthoriser);
        var signReq = new { ReportId = reportId, AuthoriserId = "auth1", Comments = "OK" };
        var signResp = await client.PostAsJsonAsync("/api/Reports/eod/signoff", signReq);
        signResp.EnsureSuccessStatusCode();
    }
}
