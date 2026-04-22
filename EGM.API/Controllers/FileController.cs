using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.RateLimiting;

namespace EGM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileController> _logger;

        public FileController(IWebHostEnvironment env, ILogger<FileController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [HttpPost("upload")]
        [EnableRateLimiting("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            // İzin verilen uzantılar
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".tiff", ".tif", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Desteklenmeyen dosya formatı.");

            if (file.Length > 10 * 1024 * 1024) // 10MB sınırı
                return BadRequest("Dosya boyutu çok büyük (Max: 10MB).");

            // Magic Bytes Taraması (Zararlı Dosya Koruması)
            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                var signatures = new Dictionary<string, List<byte[]>>
                {
                    { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
                    { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
                    { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
                    { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
                    { ".tiff", new List<byte[]> { new byte[] { 0x49, 0x49, 0x2A, 0x00 }, new byte[] { 0x4D, 0x4D, 0x00, 0x2A } } },
                    { ".tif",  new List<byte[]> { new byte[] { 0x49, 0x49, 0x2A, 0x00 }, new byte[] { 0x4D, 0x4D, 0x00, 0x2A } } },
                    { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
                    { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
                    { ".xlsx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }
                };

                if (signatures.TryGetValue(extension, out var expectedSignatures))
                {
                    var headerBytes = reader.ReadBytes(8);
                    bool isMatch = false;
                    foreach (var sig in expectedSignatures)
                    {
                        if (headerBytes.Take(sig.Length).SequenceEqual(sig))
                        {
                            isMatch = true;
                            break;
                        }
                    }
                    if (!isMatch)
                    {
                        _logger.LogWarning("Zararlı dosya yükleme girişimi: {FileName}", file.FileName);
                        return BadRequest("Dosya içeriği manipüle edilmiş! Yükleme reddedildi.");
                    }
                }
            }

            try
            {
                var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Benzersiz dosya adı oluştur
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Dosya akışını sıfırla çünkü magic byte için okuduk
                file.OpenReadStream().Position = 0;

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Dosya yüklendi: {FileName}", fileName);

                // Geri dönüş URL'i: Artık app.UseStaticFiles olmadığı için /api/file/{fileName} üzerinden dağıtılacak
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var fileUrl = $"{baseUrl}/api/file/{fileName}";

                return Ok(new { url = fileUrl, fileName = file.FileName, systemName = fileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya yüklenirken hata oluştu.");
                return StatusCode(500, "Dosya kaydedilirken sunucu tarafında bir hata oluştu.");
            }
        }

        [HttpGet("{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                return BadRequest("Geçersiz dosya adı.");

            var uploadsDir = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsDir, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Dosya bulunamadı veya silinmiş.");

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".tiff" or ".tif" => "image/tiff",
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream",
            };

            return PhysicalFile(filePath, mimeType);
        }
    }
}
