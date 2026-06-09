using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Sisal.Application;
using Sisal.Application.Common.Interfaces;
using Sisal.Application.Common.Models;
using Sisal.Domain.Enums;
using SiSal.API.Authorization;
using SiSal.API.Identity;
using SiSal.API.Middleware;
using SiSal.API.OpenApi;
using SiSal.Infrastructure;
using SiSal.Infrastructure.Persistence;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var privilegio in Enum.GetValues<TipoPrivilegio>())
    {
        options.AddPolicy(privilegio.ToString(), policy =>
            policy.RequireAuthenticatedUser()
                  .AddRequirements(new PrivilegioRequirement(privilegio)));
    }
});

builder.Services.AddScoped<IAuthorizationHandler, PrivilegioAuthorizationHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddControllers();

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<OpenApiInfoTransformer>());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

const string corsPolicy = "SisalCors";
builder.Services.AddCors(options => options.AddPolicy(corsPolicy, policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
          .AllowAnyHeader()
          .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.MapOpenApi();
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
}

app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<SisalDbInitializer>();
    await initializer.InitialiseAsync();
    await initializer.SeedAsync();
}

app.Run();
