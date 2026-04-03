using EGM.Domain.Entities;
using EGM.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace EGM.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core SaveChanges interceptor: Create / Update / Delete işlemlerini
    /// otomatik olarak AuditLog tablosuna kaydeder.
    ///
    /// Dikkat: AuditLog entity'sinin kendisi denetlenmez (sonsuz döngü önlenir).
    /// Soft-delete (IsDeleted=true) işlemleri de Delete olarak kayıt altına alınır.
    /// </summary>
    public sealed class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
                AddAuditLogs(eventData.Context);

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
                AddAuditLogs(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        // ─────────────────────────────────────────────────────────────────
        private void AddAuditLogs(DbContext context)
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                             .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? "system";

            var entries = context.ChangeTracker
                .Entries<BaseEntity>()
                .Where(e => e.Entity is not AuditLog
                            && e.State is EntityState.Added
                                       or EntityState.Modified
                                       or EntityState.Deleted)
                .ToList();

            foreach (var entry in entries)
            {
                // Yeni eklenen her kayıtta CreatedByUserId otomatik doldur
                if (entry.State == EntityState.Added
                    && string.IsNullOrEmpty(entry.Entity.CreatedByUserId))
                {
                    entry.Entity.CreatedByUserId = userId;
                }

                var action = entry.State switch
                {
                    EntityState.Added    => AuditAction.Create,
                    EntityState.Deleted  => AuditAction.Delete,
                    // Modified: soft-delete (IsDeleted=true → Deleted), yoksa Update
                    EntityState.Modified => IsSoftDelete(entry) ? AuditAction.Delete : AuditAction.Update,
                    _                    => AuditAction.Update
                };

                var isSelfCorrection = action == AuditAction.Update
                    && !string.IsNullOrEmpty(entry.Entity.CreatedByUserId)
                    && entry.Entity.CreatedByUserId == userId;

                var changes = BuildChangesJson(entry, action, isSelfCorrection);

                var log = new AuditLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId   = entry.Entity.Id,
                    Action     = action,
                    UserId     = userId,
                    Timestamp  = DateTime.UtcNow,
                    Changes    = changes
                };

                context.Set<AuditLog>().Add(log);
            }
        }

        /// <summary>Yalnızca IsDeleted alanı true'ya çekilen Modified kayıtlarını yakalar.</summary>
        private static bool IsSoftDelete(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry)
        {
            if (entry.State != EntityState.Modified) return false;
            var isDeletedProp = entry.Property(nameof(BaseEntity.IsDeleted));
            return isDeletedProp.IsModified
                   && isDeletedProp.CurrentValue is true
                   && isDeletedProp.OriginalValue is false;
        }

        /// <summary>
        /// Create → sadece yeni değerler.
        /// Update → eski ve yeni değer farklı olan alanlar.
        /// Delete → eski değerler.
        /// isSelfCorrection=true ise Changes JSON'a "selfCorrection: true" notu eklenir.
        /// </summary>
        private static string BuildChangesJson(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry,
            AuditAction action,
            bool isSelfCorrection = false)
        {
            // Denetim amacıyla hassas olmayan property'leri topla
            var dict = new Dictionary<string, object?>();

            // Kendi hatasını düzelten personelin işlemini özel olarak etiketle
            if (isSelfCorrection)
                dict["_note"] = "Self-Correction";

            foreach (var prop in entry.Properties)
            {
                var name = prop.Metadata.Name;

                // BaseEntity altyapı alanlarını atla
                if (name is nameof(BaseEntity.CreatedAt)
                         or nameof(BaseEntity.UpdatedAt))
                    continue;

                switch (action)
                {
                    case AuditAction.Create:
                        dict[name] = prop.CurrentValue;
                        break;

                    case AuditAction.Delete:
                        dict[name] = prop.OriginalValue;
                        break;

                    case AuditAction.Update:
                        if (prop.IsModified)
                            dict[name] = new { from = prop.OriginalValue, to = prop.CurrentValue };
                        break;
                }
            }

            return JsonSerializer.Serialize(dict);
        }
    }
}