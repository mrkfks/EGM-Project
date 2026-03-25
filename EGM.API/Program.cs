using EGM.Application.Services;
using EGM.API.Middleware;
using EGM.Domain.Constants;
using EGM.Domain.Interfaces;
using EGM.Infrastructure;
using EGM.Infrastructure.Hubs;
using EGM.Infrastructure.Persistence;
using EGM.Infrastructure.Repositories;
using EGM.Infrastructure.Security;
using EGM.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// JWT anahtarını al ve kontrol et
var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(key))
    throw new InvalidOperationException("JWT key yapılandırılmamış. Ortam değişkeni: Jwt__Key");

// Şifreleme anahtarlarını kontrol et
var encKey = builder.Configuration["Encryption:Key"];
var encIv  = builder.Configuration["Encryption:IV"];
if (string.IsNullOrEmpty(encKey) || string.IsNullOrEmpty(encIv))
    throw new InvalidOperationException("Şifreleme anahtarları yapılandırılmamış. Ortam değişkenleri: Encryption__Key, Encryption__IV");

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

    // SignalR WebSocket bağlantılarında token query string'den okunur
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Hepsi: sisteme girebilen herkes (Izleyici dahil)
    options.AddPolicy("Viewer", p =>
        p.RequireAuthenticatedUser());

    // Veri girebilen: İl Personeli ve üzeri
    options.AddPolicy("CityStaffOrAbove", p =>
        p.RequireRole(
            Roles.IlPersoneli, Roles.IlYoneticisi,
            Roles.BaskanlikPersoneli, Roles.BaskanlikYoneticisi));

    // Şehir yöneticisi ve üzeri: onay/yönetim
    options.AddPolicy("CityManagerOrAbove", p =>
        p.RequireRole(
            Roles.IlYoneticisi,
            Roles.BaskanlikPersoneli, Roles.BaskanlikYoneticisi));

    // Yalnızca Başkanlık rolleri (coğrafi kısıtsız)
    options.AddPolicy("HQOnly", p =>
        p.RequireRole(Roles.BaskanlikPersoneli, Roles.BaskanlikYoneticisi));

    // Yalnızca Süper Admin
    options.AddPolicy("HQManagerOnly", p =>
        p.RequireRole(Roles.BaskanlikYoneticisi));
});

// ── HttpContext Accessor (AuditInterceptor için) ─────────────────────────
builder.Services.AddHttpContextAccessor();

// ── SignalR ──────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Encryption Servisi ───────────────────────────────────────────────────
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

// ── Audit Interceptor ────────────────────────────────────────────────────
builder.Services.AddScoped<AuditInterceptor>();

// ── Veritabanı ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<EGMDbContext>((serviceProvider, options) =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
});

// ── Generic Repository ───────────────────────────────────────────────────
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

// ── Kullanıcı Deposu ─────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
// ── Olay Deposu (eager loading) ───────────────────────────────────────
builder.Services.AddScoped<IOlayRepository, OlayRepository>();
// ── Infrastructure Servisleri ─────────────────────────────────────────────
builder.Services.AddScoped<JwtTokenService>();builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
// ── Application Servisleri ───────────────────────────────────────────────
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<RoleAssignmentService>();
builder.Services.AddScoped<OlayService>();
builder.Services.AddScoped<OperasyonelFaaliyetService>();
builder.Services.AddScoped<OrganizatorService>();
builder.Services.AddScoped<SecimService>();
builder.Services.AddScoped<SehitService>();
builder.Services.AddScoped<OluService>();
builder.Services.AddScoped<SosyalMedyaOlayService>();
builder.Services.AddScoped<SupheliService>();
builder.Services.AddScoped<VIPZiyaretService>();

// ── Bildirim Servisi ─────────────────────────────────────────────────────
builder.Services.AddScoped<IInAppNotificationService, InAppNotificationService>();

// ── Rate Limiting ────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // Login endpoint: 15 dakikada en fazla 5 deneme
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(15);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Controllers ve Swagger ───────────────────────────────────────────────
// ── CORS ─────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// ── Health Checks ────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EGMDbContext>();

// ── Response Compression ────────────────────────────────────────────────
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Optimal);

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

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseHsts();
app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health").AllowAnonymous();

// ── Başlangıçta DB bağlantısını doğrula ─────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EGMDbContext>();
    if (!await db.Database.CanConnectAsync())
        throw new InvalidOperationException("Veritabanına bağlanılamadı. Bağlantı dizesini kontrol edin.");
}

app.Run();

