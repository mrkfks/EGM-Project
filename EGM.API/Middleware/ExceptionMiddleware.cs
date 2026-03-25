using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace EGM.API.Middleware
{
    public sealed class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Kayıt bulunamadı.");
                await WriteProblemAsync(context, HttpStatusCode.NotFound,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    "Kayıt Bulunamadı", ex.Message.Length > 0 ? ex.Message : "İstenen kayıt bulunamadı.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Yetkisiz erişim girişimi.");
                await WriteProblemAsync(context, HttpStatusCode.Forbidden,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    "Erişim Reddedildi", "Bu işlem için yetkiniz bulunmamaktadır.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Geçersiz işlem.");
                await WriteProblemAsync(context, HttpStatusCode.BadRequest,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    "Geçersiz İşlem", ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Geçersiz argüman.");
                await WriteProblemAsync(context, HttpStatusCode.BadRequest,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    "Geçersiz Parametre", ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Eş zamanlılık çakışması.");
                await WriteProblemAsync(context, HttpStatusCode.Conflict,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    "Çakışma", "Kayıt başka bir kullanıcı tarafından değiştirildi. Lütfen sayfayı yenileyip tekrar deneyin.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Veritabanı güncelleme hatası.");
                // Unique constraint ihlali gibi durumları kullanıcıya net hata ver
                var msg = ex.InnerException?.Message ?? ex.Message;
                var isDuplicate = msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                               || msg.Contains("unique constraint", StringComparison.OrdinalIgnoreCase);

                await WriteProblemAsync(context,
                    isDuplicate ? HttpStatusCode.Conflict : HttpStatusCode.UnprocessableEntity,
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    isDuplicate ? "Tekrar Eden Kayıt" : "Veritabanı Hatası",
                    isDuplicate ? "Bu kayıt zaten mevcut." : "Kayıt işlemi sırasında bir hata oluştu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İşlenmeyen hata.");
                await WriteProblemAsync(context, HttpStatusCode.InternalServerError,
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    "Sunucu Hatası", "Sunucu tarafında bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
            }
        }

        private static Task WriteProblemAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string type,
            string title,
            string detail)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode  = (int)statusCode;

            var body = JsonSerializer.Serialize(new
            {
                type,
                title,
                status = (int)statusCode,
                detail,
                instance = context.Request.Path.Value
            });

            return context.Response.WriteAsync(body);
        }
    }
}
