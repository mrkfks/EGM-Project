using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EGM.Domain.Enums;

namespace EGM.Application.DTOs
{
    // ── Sayfalama ────────────────────────────────────────────────────────
    /// <summary>Sayfalanmış sonuçlar için standart sarmalayıcı.</summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    // ── Olay ────────────────────────────────────────────────────────────
    public class OlayCreateDto
    {
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(250, ErrorMessage = "Başlık en fazla 250 karakter olabilir.")]
        public string? Baslik { get; set; }

        [Required(ErrorMessage = "Olay türü zorunludur.")]
        [StringLength(100)]
        public string? OlayTuru { get; set; }

        [Required]
        public Guid OrganizatorId { get; set; }

        [Required]
        public Guid KonuId { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }

        [Required(ErrorMessage = "İl zorunludur.")]
        [StringLength(100)]
        public string? Il { get; set; }

        [StringLength(100)]
        public string? Ilce { get; set; }

        [StringLength(250)]
        public string? Mekan { get; set; }

        [Range(-90.0, 90.0, ErrorMessage = "Enlem -90 ile 90 arasında olmalıdır.")]
        public double? Latitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "Boylam -180 ile 180 arasında olmalıdır.")]
        public double? Longitude { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Katılımcı sayısı negatif olamaz.")]
        public int? KatilimciSayisi { get; set; }

        [StringLength(1000)]
        public string? Aciklama { get; set; }

        [StringLength(250)]
        public string? KaynakKurum { get; set; }

        public Hassasiyet Hassasiyet { get; set; }
        /// <summary>Başkanlık personeli bu alanı göndererek il belirtir. İl personeli için servis otomatik atar.</summary>
        public int? CityId { get; set; }
    }

    public class OlayResponseDto
    {
        public Guid Id { get; set; }
        public string? Baslik { get; set; }
        public string? OlayTuru { get; set; }
        public Guid OrganizatorId { get; set; }
        public Guid KonuId { get; set; }
        public DateTime Tarih { get; set; }
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? Mekan { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? KatilimciSayisi { get; set; }
        public string? Aciklama { get; set; }
        public string? KaynakKurum { get; set; }
        public OlayDurum Durum { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public double RiskPuani { get; set; }
        public DateTime? GercekBaslangicTarihi { get; set; }
        public DateTime? GercekBitisTarihi { get; set; }
        /// <summary>Kaydı oluşturan kullanıcının sicil numarası (RBAC sahiplik kontrolü için).</summary>
        public string CreatedByUserId { get; set; } = string.Empty;
        /// <summary>Olayın gerçekleştiği ilin plaka kodu.</summary>
        public int? CityId { get; set; }
    }

    // ── Risk Preview ────────────────────────────────────────────────────────
    public class RiskPreviewRequestDto
    {
        public int? KatilimciSayisi { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        [StringLength(100)]
        public string? OlayTuru { get; set; }
        [Range(0.0, 100.0, ErrorMessage = "Sosyal sinyal skoru 0-100 arasında olmalıdır.")]
        public double SosyalSignalSkoru { get; set; }
    }

    public class RiskPreviewResponseDto
    {
        public double RiskPuaniRaw { get; set; }
        public double RiskPuaniNormalized { get; set; }
        /// <summary>Düşük / Orta / Yüksek / Kritik</summary>
        public string Seviye { get; set; } = string.Empty;
    }

    // ── OperasyonelFaaliyet ──────────────────────────────────────────────
    public class OperasyonelFaaliyetCreateDto
    {
        [Required]
        public Guid OlayId { get; set; }

        [StringLength(1000)]
        public string? Aciklama { get; set; }
    }

    public class OperasyonelFaaliyetResponseDto
    {
        public Guid Id { get; set; }
        public Guid OlayId { get; set; }
        public string? Aciklama { get; set; }
        public int ToplamGrupSayisi { get; set; }
        public int SupheliSayisi { get; set; }
        public int GozaltiSayisi { get; set; }
        public int SehitSayisi { get; set; }
        public int OluSayisi { get; set; }
    }

    public class KatilimciGrupCreateDto
    {
        [Required(ErrorMessage = "Grup adı zorunludur.")]
        [StringLength(250)]
        public string? GrupAdi { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Katılımcı sayısı negatif olamaz.")]
        public int KatilimciSayisi { get; set; }
    }

    // ── Supheli ─────────────────────────────────────────────────────────
    public class SupheliCreateDto
    {
        [Required]
        public Guid OperasyonelFaaliyetId { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(100)]
        public string? Ad { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(100)]
        public string? Soyad { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No 11 haneli rakamdan oluşmalıdır.")]
        public string? TcKimlikNo { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        public DateTime DogumTarihi { get; set; }

        public bool Gozaltinda { get; set; }
    }

    public class SupheliResponseDto
    {
        public Guid Id { get; set; }
        public Guid OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public bool Gozaltinda { get; set; }
        // TC Kimlik No kasitli olarak response'a dahil edilmedi (gizlilik)
    }

    // ── Sehit ───────────────────────────────────────────────────────────
    public class SehitCreateDto
    {
        [Required]
        public Guid OperasyonelFaaliyetId { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(100)]
        public string? Ad { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(100)]
        public string? Soyad { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No 11 haneli rakamdan oluşmalıdır.")]
        public string? TcKimlikNo { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        public DateTime DogumTarihi { get; set; }

        [StringLength(250)]
        public string? Gorev { get; set; }
    }

    public class SehitResponseDto
    {
        public Guid Id { get; set; }
        public Guid OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? Gorev { get; set; }
    }

    // ── Olu ─────────────────────────────────────────────────────────────
    public class OluCreateDto
    {
        [Required]
        public Guid OperasyonelFaaliyetId { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(100)]
        public string? Ad { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [StringLength(100)]
        public string? Soyad { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No 11 haneli rakamdan oluşmalıdır.")]
        public string? TcKimlikNo { get; set; }

        [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
        public DateTime DogumTarihi { get; set; }

        [StringLength(250)]
        public string? KatilimciDurumu { get; set; }
    }

    public class OluResponseDto
    {
        public Guid Id { get; set; }
        public Guid OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? KatilimciDurumu { get; set; }
    }

    // ── Sosyal Medya Olayi ───────────────────────────────────────────────
    public class SosyalMedyaOlayCreateDto
    {
        [Required]
        public Guid OlayId { get; set; }

        [Required(ErrorMessage = "Platform zorunludur.")]
        [StringLength(100)]
        public string? Platform { get; set; }

        [Required(ErrorMessage = "Paylaşım linki zorunludur.")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [StringLength(2000)]
        public string? PaylasimLinki { get; set; }

        [Required(ErrorMessage = "Paylaşım tarihi zorunludur.")]
        public DateTime PaylasimTarihi { get; set; }

        [StringLength(2000)]
        public string? IcerikOzeti { get; set; }

        [StringLength(250)]
        public string? IlgiliKisiKurum { get; set; }

        public Hassasiyet Hassasiyet { get; set; }

        [Range(0.0, 100.0, ErrorMessage = "Sosyal sinyal skoru 0-100 arasında olmalıdır.")]
        public double SosyalSignalSkoru { get; set; }
    }

    public class SosyalMedyaOlayResponseDto
    {
        public Guid Id { get; set; }
        public Guid OlayId { get; set; }
        public string? Platform { get; set; }
        public string? PaylasimLinki { get; set; }
        public DateTime PaylasimTarihi { get; set; }
        public string? IcerikOzeti { get; set; }
        public string? IlgiliKisiKurum { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public double SosyalSignalSkoru { get; set; }
    }

    // ── Organizator ──────────────────────────────────────────────────────
    public class OrganizatorCreateDto
    {
        [Required(ErrorMessage = "Organizatör adı zorunludur.")]
        [StringLength(250)]
        public string? Ad { get; set; }

        [Required(ErrorMessage = "Kuruluş tarihi zorunludur.")]
        public DateTime KurulusTarihi { get; set; }

        [StringLength(500)]
        public string? FaaliyetAlani { get; set; }

        [StringLength(500)]
        public string? Iletisim { get; set; }
    }

    public class OrganizatorResponseDto
    {
        public Guid Id { get; set; }
        public string? Ad { get; set; }
        public DateTime KurulusTarihi { get; set; }
        public string? FaaliyetAlani { get; set; }
        public string? Iletisim { get; set; }
    }

    public class KonuCreateDto
    {
        [Required(ErrorMessage = "Konu adı zorunludur.")]
        [StringLength(250)]
        public string? Ad { get; set; }

        [StringLength(1000)]
        public string? Aciklama { get; set; }
    }

    // ── Secim ────────────────────────────────────────────────────────────
    public class SecimSonucuCreateDto
    {
        [Required(ErrorMessage = "Seçim türü zorunludur.")]
        [StringLength(100)]
        public string? SecimTuru { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Bölge tipi zorunludur.")]
        [StringLength(100)]
        public string? BolgeTipi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir BölgeId giriniz.")]
        public int BolgeId { get; set; }

        [Required]
        public Guid AdayId { get; set; }

        [Required]
        public Guid PartiId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Oy sayısı negatif olamaz.")]
        public int OySayisi { get; set; }

        [Range(0.0, 100.0, ErrorMessage = "Oy oranı 0-100 arasında olmalıdır.")]
        public double OyOrani { get; set; }

        [Required]
        public Guid KaynakId { get; set; }

        public bool KaynakOnayDurumu { get; set; }
    }

    public class SecimSonucuResponseDto
    {
        public Guid Id { get; set; }
        public string? SecimTuru { get; set; }
        public DateTime Tarih { get; set; }
        public string? BolgeTipi { get; set; }
        public int BolgeId { get; set; }
        public Guid AdayId { get; set; }
        public Guid PartiId { get; set; }
        public int OySayisi { get; set; }
        public double OyOrani { get; set; }
        public bool KaynakOnayDurumu { get; set; }
    }

    public class AdayCreateDto
    {
        [Required(ErrorMessage = "Aday adı-soyadı zorunludur.")]
        [StringLength(250)]
        public string? AdSoyad { get; set; }

        [Required(ErrorMessage = "Parti ismi zorunludur.")]
        [StringLength(250)]
        public string? PartiAdi { get; set; }
    }

    public class PartiCreateDto
    {
        [Required(ErrorMessage = "Parti adı zorunludur.")]
        [StringLength(250)]
        public string? Ad { get; set; }
    }

    // ── VIP Ziyaret ──────────────────────────────────────────────────────
    public class VIPZiyaretCreateDto : IValidatableObject
    {
        [Required(ErrorMessage = "Ziyaret eden ad-soyad zorunludur.")]
        [StringLength(250)]
        public string? ZiyaretEdenAdSoyad { get; set; }

        [StringLength(250)]
        public string? Unvan { get; set; }

        [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
        public DateTime BaslangicTarihi { get; set; }

        [Required(ErrorMessage = "Bitiş tarihi zorunludur.")]
        public DateTime BitisTarihi { get; set; }

        [Required(ErrorMessage = "İl zorunludur.")]
        [StringLength(100)]
        public string? Il { get; set; }

        [StringLength(250)]
        public string? Mekan { get; set; }

        public Hassasiyet Hassasiyet { get; set; }

        [StringLength(100)]
        public string? GuvenlikSeviyesi { get; set; }

        [StringLength(2000)]
        public string? GozlemNoktalari { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (BitisTarihi <= BaslangicTarihi)
                yield return new ValidationResult(
                    "Bitiş tarihi başlangıç tarihinden sonra olmalıdır.",
                    new[] { nameof(BitisTarihi) });
        }
    }

    public class VIPZiyaretResponseDto
    {
        public Guid Id { get; set; }
        public string? ZiyaretEdenAdSoyad { get; set; }
        public string? Unvan { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public string? Il { get; set; }
        public string? Mekan { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public string? GuvenlikSeviyesi { get; set; }
        public string? GozlemNoktalari { get; set; }
    }

    public class GuvenlikPlaniCreateDto
    {
        [Required]
        public Guid VIPZiyaretId { get; set; }

        [Required(ErrorMessage = "Güvenlik planı adı zorunludur.")]
        [StringLength(250)]
        public string? Ad { get; set; }

        [StringLength(2000)]
        public string? Aciklama { get; set; }
    }

    public class EkipCreateDto
    {
        [Required]
        public Guid VIPZiyaretId { get; set; }

        [Required(ErrorMessage = "Ekip adı zorunludur.")]
        [StringLength(250)]
        public string? Ad { get; set; }
    }

    // ── Yuruyu Rotasi ───────────────────────────────────────────────────
    public class RotaNoktasiCreateDto
    {
        [StringLength(250)]
        public string? NoktaAdi { get; set; }

        [Required]
        [Range(-90.0, 90.0, ErrorMessage = "Enlem -90 ile 90 arasında olmalıdır.")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180.0, 180.0, ErrorMessage = "Boylam -180 ile 180 arasında olmalıdır.")]
        public double Longitude { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Sıra numarası en az 1 olmalıdır.")]
        public int SiraNo { get; set; }
    }

    // ── Kullanici ────────────────────────────────────────────────────────
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public int Sicil { get; set; }
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
    }
    /// <summary>Rol atama işlemi için request body DTO'su.</summary>
    public class RolAtamaDto
    {
        /// <summary>
        /// Hedef kullanıcıya atanacak yeni rol.
        /// Geçerli değerler: Izleyici, IlPersoneli, IlYoneticisi,
        /// BaskanlikPersoneli, BaskanlikYoneticisi
        /// </summary>
        public string YeniRol { get; set; } = string.Empty;

        /// <summary>Başkanlık Yöneticisi tarafından il personeli atanmasında il plaka kodu.</summary>
        public int? CityId { get; set; }
    }}
