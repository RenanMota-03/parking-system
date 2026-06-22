using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using ParkingSystem.Module.Identity.Infra.Data.EF;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Shared.IoC;
using ParkingSystem.WebApi.Bff.WebApp.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection não configurada. " +
        "Em produção, defina a variável de ambiente ConnectionStrings__DefaultConnection.");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

var sniHost = builder.Configuration["Database:SniHost"];
if (!string.IsNullOrWhiteSpace(sniHost))
    dataSourceBuilder.UseSslClientAuthenticationOptionsCallback(ssl => ssl.TargetHost = sniHost);

var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(dataSource));
builder.Services.AddDbContext<ParkingDbContext>(options => options.UseNpgsql(dataSource));

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAppPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

NativeInjectorBootStrapper.RegisterServices(builder.Services);

builder.Services.AddOpenApi();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret  = jwtSection["Secret"];
var jwtIssuer  = jwtSection["Issuer"]!;
var jwtAudience = jwtSection["Audience"]!;

if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Secret não configurado ou muito curto (mínimo 32 caracteres). " +
        "Em produção, defina a variável de ambiente Jwt__Secret.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("WebAppPolicy");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("reference");
}

app.MapAuthEndpoints();
app.MapVagaEndpoints();
app.MapTarifaEndpoints();
app.MapMovimentacaoEndpoints();
app.MapReservaEndpoints();
app.MapRelatoriosEndpoints();
app.MapUsuariosEndpoints();

app.Run();
