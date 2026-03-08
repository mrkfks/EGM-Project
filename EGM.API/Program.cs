using EGM.Application.Services;
using EGM.Domain.Interfaces;
using EGM.Infrastructure;
using EGM.Infrastructure.Persistence;
using EGM.Infrastructure.Repositories;
using EGM.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT anahtarını al ve kontrol et
var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(key))
    throw new InvalidOperationException("JWT key is not configured.");

var issuer   = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

// ── JWT Authentication ────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = issuer,
        ValidAudience            = audience,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
    };
});

builder.Services.AddAuthorization();

// ── Veritabanı ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<EGMDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Generic Repository ───────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// ── Kullanıcı Deposu ─────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ── Infrastructure Servisleri ─────────────────────────────────────────────
builder.Services.AddScoped<JwtTokenService>();

// ── Application Servisleri ───────────────────────────────────────────────
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<OlayService>();
builder.Services.AddScoped<OperasyonelFaaliyetService>();
builder.Services.AddScoped<OrganizatorService>();
builder.Services.AddScoped<SecimService>();
builder.Services.AddScoped<SehitService>();
builder.Services.AddScoped<OluService>();
builder.Services.AddScoped<SosyalMedyaOlayService>();
builder.Services.AddScoped<SupheliService>();
builder.Services.AddScoped<VIPZiyaretService>();

// ── Controllers ve Swagger ───────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware ────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

