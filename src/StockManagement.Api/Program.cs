using System.Text;
using Microsoft.OpenApi;
using StockManagement.Api;
using StockManagement.Application;
using StockManagement.Application.Services;
using StockManagement.Domain;
using StockManagement.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using StockManagement.Infrastructure.Persistence;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StockManagement API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    // c.AddSecurityRequirement(new OpenApiSecurityRequirement
    // {
    //     {
    //         new OpenApiSecurityScheme
    //         {
    //             Reference = new OpenApiReference
    //             {
    //                 Type = ReferenceType.SecurityScheme,
    //                 Id = "Bearer"
    //             }
    //         },
    //         new string[] { }
    //     }
    // });
});

// Authentication configuration. By default use a mock JWT handler that decodes token payload without signature
// verification (useful when tokens come from an external auth microservice). Set USE_MOCK_JWT=false
// to enable strict JwtBearer validation when JWT_SECRET etc are configured.
var useMock = (Environment.GetEnvironmentVariable("USE_MOCK_JWT") ?? builder.Configuration.GetValue<string>("UseMockJwt") ?? "true").ToLowerInvariant() != "false";
if (useMock)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "MockJwt";
        options.DefaultChallengeScheme = "MockJwt";
    }).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, StockManagement.Api.Authentication.MockJwtAuthenticationHandler>("MockJwt", options => { });
}
else
{
    // Strict JwtBearer validation path
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_super_secret_long_default_for_development_2026!!";
    var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "stockmanagement";
    var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "stockmanagement_audience";
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.FromSeconds(5)
        };
    });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InputterPolicy", policy => policy.RequireRole("Inputter"));
    options.AddPolicy("AuthoriserPolicy", policy => policy.RequireRole("Authoriser"));
    options.AddPolicy("ReconcilerPolicy", policy => policy.RequireRole("Reconciler"));
});

// Configure DbContext based on configuration (appsettings or env vars)
var persistenceProvider = builder.Configuration.GetSection("Persistence").GetValue<string>("Provider") ?? "InMemory";
if (persistenceProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<StockDbContext>(opt => opt.UseSqlServer(conn));
}
else if (persistenceProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
{
    var conn = builder.Configuration.GetConnectionString("Postgres");
    builder.Services.AddDbContext<StockDbContext>(opt => opt.UseNpgsql(conn));
}
else
{
    builder.Services.AddDbContext<StockDbContext>(opt => opt.UseInMemoryDatabase("StockDb"));
}
builder.Services.AddScoped<ControlledItemRepository>();
builder.Services.AddScoped<ControlledItemService>();
builder.Services.AddScoped<ControlledTransactionRepository>();
builder.Services.AddScoped<OpeningBalanceRepository>();
builder.Services.AddScoped<ReconciliationService>();
builder.Services.AddScoped<AgeingService>();
builder.Services.AddScoped<StockManagement.Domain.Interfaces.INotificationPublisher, StockManagement.Infrastructure.Services.EmailNotificationPublisher>();
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<ReportService>();
// MediatR (scan Application assembly for handlers)
builder.Services.AddMediatR(typeof(StockManagement.Application.Services.ControlledItemService).Assembly);
builder.Services.AddScoped<CaptureReasonRepository>();
builder.Services.AddScoped<StockManagement.Domain.Interfaces.IAccountLookup, StockManagement.Infrastructure.Services.SimpleAccountLookup>();
builder.Services.AddScoped<StockManagement.Infrastructure.Repositories.InventorySnapshotRepository>();

// Scheduler
builder.Services.AddHostedService<StockManagement.Infrastructure.Services.SchedulerHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Run automatic seeding on startup
try
{
    var useSeeder = builder.Configuration.GetValue<bool?>("RunSeeder") ?? true;
    if (useSeeder)
    {
        await StockManagement.Infrastructure.Seeding.DataSeeder.SeedAsync(app.Services);
    }
}
catch (Exception ex)
{
    // don't crash startup on seeder errors in development
    app.Logger.LogWarning(ex, "Data seeding failed");
}

app.Run();
