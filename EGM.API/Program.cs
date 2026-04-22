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
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using BCrypt.Net;
using System.Linq;

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
            if (!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/hubs") || path.StartsWithSegments("/api/file")))
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
            Roles.BaskanlikPersoneli, Roles.BaskanlikYoneticisi, Roles.Yetkili));

    // Şehir yöneticisi ve üzeri: onay/yönetim
    options.AddPolicy("CityManagerOrAbove", p =>
        p.RequireRole(
            Roles.IlYoneticisi,
            Roles.BaskanlikPersoneli, Roles.BaskanlikYoneticisi, Roles.Yetkili));

    // Yalnızca Başkanlık rolleri (coğrafi kısıtsız)
    options.AddPolicy("HQOnly", p =>
        p.RequireRole(Roles.BaskanlikPersoneli, Roles.BaskanlikYoneticisi, Roles.Yetkili));

    // Yalnızca Başkanlık Yöneticisi ve Yetkili
    options.AddPolicy("HQManagerOnly", p =>
        p.RequireRole(Roles.BaskanlikYoneticisi, Roles.Yetkili));
});

// ── HttpContext Accessor (AuditInterceptor için) ─────────────────────────
builder.Services.AddHttpContextAccessor();

// ── SignalR ──────────────────────────────────────────────────────────────
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 64 * 1024; // 64 KB
    options.KeepAliveInterval         = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval     = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout          = TimeSpan.FromSeconds(5);
});

// ── Encryption Servisi ───────────────────────────────────────────────────
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

// ── Audit Interceptor ────────────────────────────────────────────────────
builder.Services.AddScoped<AuditInterceptor>();

// ── Veritabanı ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<EGMDbContext>((serviceProvider, options) =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
    if (builder.Environment.IsDevelopment())
        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Warning);
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
builder.Services.AddScoped<IOlayService, OlayService>();
builder.Services.AddScoped<OrganizatorService>();
builder.Services.AddScoped<RaporlarService>();

// ── Coğrafi Veri Servisi (TurkiyeRehber.sqlite) ──────────────────────────
builder.Services.AddSingleton<IGeoService, GeoDbService>();
builder.Services.AddScoped<IGeoAreaService, GeoAreaService>();

// ── Bildirim Servisi ─────────────────────────────────────────────────────
builder.Services.AddScoped<IInAppNotificationService, InAppNotificationService>();

// ── Arka Plan Servisleri ─────────────────────────────────────────────────
builder.Services.AddHostedService<EGM.API.BackgroundServices.OlayStartNotifierService>();

// ── Rate Limiting ────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // Login endpoint: development'ta sınırsız, production'da 15 dakikada 5 deneme
    var isDev = builder.Environment.IsDevelopment();
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = isDev ? 1000 : 5;
        opt.Window = TimeSpan.FromMinutes(isDev ? 1 : 15);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("upload", opt =>
    {
        opt.PermitLimit = isDev ? 100 : 10;
        opt.Window = TimeSpan.FromMinutes(1);
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
    {
        if (builder.Environment.IsDevelopment())
        {
            // Geliştirme modunda herhangi bir localhost/IP:4200 origin'e izin ver
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                (uri.Port == 4200 || uri.Port == 4201))
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
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

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Model validation hatalarını loglat (geliştirme aşaması için)
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    var builtInFactory = options.InvalidModelStateResponseFactory;
    options.InvalidModelStateResponseFactory = context =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILogger<Program>>();
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .Select(e => $"{e.Key}: {string.Join(", ", e.Value!.Errors.Select(x => x.ErrorMessage))}");
        logger.LogWarning("Model validation hatası [{Path}]: {Errors}",
            context.HttpContext.Request.Path,
            string.Join(" | ", errors));
        return builtInFactory(context);
    };
});

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

// Ensure responses explicitly include charset=utf-8 when appropriate
app.Use(async (context, next) =>
{
    context.Response.OnStarting(state =>
    {
        var httpContext = (HttpContext)state!;
        try
        {
            if (httpContext.Response.HasStarted) return Task.CompletedTask;

            if (httpContext.Response.Headers.TryGetValue("Content-Type", out var ctValues))
            {
                var ct = ctValues.ToString();
                if (!string.IsNullOrEmpty(ct) && !ct.Contains("charset", StringComparison.OrdinalIgnoreCase))
                {
                    if (ct.StartsWith("text/", StringComparison.OrdinalIgnoreCase) ||
                        ct.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
                        ct.StartsWith("application/javascript", StringComparison.OrdinalIgnoreCase) ||
                        ct.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase))
                    {
                        try { httpContext.Response.Headers["Content-Type"] = ct + "; charset=utf-8"; } catch { }
                    }
                }
            }
        }
        catch { }
        return Task.CompletedTask;
    }, context);

    await next();
});
app.UseCors("AllowFrontend");
// app.UseStaticFiles(); // Kaldırıldı: Dosyalar yetkisiz erişime kapatıldı.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = false,
    ResponseWriter = async (context, report) =>
    {
        context.Response.StatusCode = report.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
            ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

        // Ensure Content-Type includes charset utf-8 in a safe way
        try
        {
            if (!context.Response.Headers.ContainsKey("Content-Type"))
                context.Response.Headers.Append("Content-Type", "application/json; charset=utf-8");
            else if (!context.Response.Headers["Content-Type"].ToString().Contains("charset", StringComparison.OrdinalIgnoreCase))
                context.Response.Headers["Content-Type"] = context.Response.Headers["Content-Type"].ToString() + "; charset=utf-8";
        }
        catch { }

        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new { key = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description })
        });

        await context.Response.WriteAsync(result);
    }
}).AllowAnonymous();


// ── Başlangıçta DB doğrula + seed verileri ekle ─────────────────────────
using (var scope = app.Services.CreateScope())
{
    DataSeeder.Seed(scope.ServiceProvider);
}

app.Run("http://0.0.0.0:5117");

internal sealed class SeedUserConfig
{
    public int Sicil { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Izleyici";
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? GSM { get; set; }
    public string? Birim { get; set; }
    public int? CityId { get; set; }
}

