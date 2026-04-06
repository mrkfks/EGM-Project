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

    // ── Günlük Bülten Raporu ─────────────────────────────────────────────
    public class GunlukBultenDto
    {
        public string Tarih { get; init; } = string.Empty;
        public string SonrakiGunTarih { get; init; } = string.Empty;
        public IReadOnlyList<IcmalVeriDto> IcmalVerileri { get; init; } = [];
        public IReadOnlyList<GerceklesenDetayDto> GerceklesenDetaylar { get; init; } = [];
        public IReadOnlyList<BeklenenDetayDto> BeklenenDetaylar { get; init; } = [];
    }

    public class IcmalVeriDto
    {
        public string Tur { get; init; } = string.Empty;
        public int EylemSayisi { get; init; }
        public int KatilimSayisi { get; init; }
        public int GozaltiSayisi { get; init; }
        public int OluSayisi { get; init; }
    }

    public class GerceklesenDetayDto
    {
        public int Sn { get; init; }
        public string Il { get; init; } = string.Empty;
        public string EylemEtkinlik { get; init; } = string.Empty;
        public string Saat { get; init; } = string.Empty;
        public string OrganizeEden { get; init; } = string.Empty;
        public string Aciklama { get; init; } = string.Empty;
        public int KatilimSayisi { get; init; }
    }

    public class BeklenenDetayDto
    {
        public int Sn { get; init; }
        public string Il { get; init; } = string.Empty;
        public string Yer { get; init; } = string.Empty;
        public string EylemEtkinlik { get; init; } = string.Empty;
        public string Saat { get; init; } = string.Empty;
        public string OrganizeEden { get; init; } = string.Empty;
        public string Aciklama { get; init; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────────────

    // ── Olay ────────────────────────────────────────────────────────────
    public class OlayCreateDto
    {
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

        [Range(0, int.MaxValue)]
        public int? GozaltiSayisi { get; set; }

        [Range(0, int.MaxValue)]
        public int? SehitOluSayisi { get; set; }

        [StringLength(1000)]
        public string? Aciklama { get; set; }

        [StringLength(100)]
        public string? EvrakNumarasi { get; set; }

        public Hassasiyet Hassasiyet { get; set; }
        /// <summary>Başkanlık personeli bu alanı göndererek il belirtir. İl personeli için servis otomatik atar.</summary>
        public int? CityId { get; set; }
        /// <summary>Olay durumu: 0=Planlandi (varsayılan), 1=Gerceklesti</summary>
        public OlayDurum Durum { get; set; } = OlayDurum.Planlandi;

        /// <summary>Formdan girilen planlanan bitiş tarihi.</summary>
        public DateTime? OlayBitisTarihi { get; set; }

        /// <summary>Etkinlik sonunda gerçekleşen katılımcı sayısı.</summary>
        [Range(0, int.MaxValue)]
        public int? GerceklesenKatilimciSayisi { get; set; }

        /// <summary>Gerçekleşme şekli FK Id'si.</summary>
        public Guid? GerceklesmeSekliId { get; set; }
    }

    public class OlayResponseDto
    {
        public Guid Id { get; set; }
        public string? OlayTuru { get; set; }
        public Guid OrganizatorId { get; set; }
        public string? OrganizatorAd { get; set; }
        public Guid KonuId { get; set; }
        public string? KonuAd { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? Mekan { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? KatilimciSayisi { get; set; }
        public int? GozaltiSayisi { get; set; }
        public int? SehitOluSayisi { get; set; }
        public string? Aciklama { get; set; }
        public string? EvrakNumarasi { get; set; }
        public OlayDurum Durum { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public DateTime? GercekBaslangicTarihi { get; set; }
        public DateTime? GercekBitisTarihi { get; set; }
        /// <summary>Kaydı oluşturan kullanıcının sicil numarası (RBAC sahiplik kontrolü için).</summary>
        public string CreatedByUserId { get; set; } = string.Empty;
        /// <summary>Olayın gerçekleştiği ilin plaka kodu.</summary>
        public int? CityId { get; set; }
        public DateTime? OlayBitisTarihi { get; set; }
        public int? GerceklesenKatilimciSayisi { get; set; }
        public Guid? GerceklesmeSekliId { get; set; }
    }

    // ── Sosyal Medya Olayi ───────────────────────────────────────────────
    public class SosyalMedyaOlayCreateDto
    {
        public Guid? OlayId { get; set; }

        [Required(ErrorMessage = "Platform zorunludur.")]
        [StringLength(100)]
        public string? Platform { get; set; }

        [StringLength(250)]
        public string? Konu { get; set; }

        [Required(ErrorMessage = "Paylaşım linki zorunludur.")]
        [StringLength(2000)]
        public string? PaylasimLinki { get; set; }

        [Required(ErrorMessage = "Paylaşım tarihi zorunludur.")]
        public DateTime PaylasimTarihi { get; set; }

        [StringLength(2000)]
        public string? IcerikOzeti { get; set; }

        [StringLength(250)]
        public string? IlgiliKisiKurum { get; set; }

        [StringLength(100)]
        public string? Il { get; set; }

        [StringLength(100)]
        public string? Ilce { get; set; }

        public string? EkranGoruntusu { get; set; }

        public Hassasiyet Hassasiyet { get; set; }
    }

    public class SosyalMedyaOlayResponseDto
    {
        public Guid Id { get; set; }
        public Guid? OlayId { get; set; }
        public string? Platform { get; set; }
        public string? Konu { get; set; }
        public string? PaylasimLinki { get; set; }
        public DateTime PaylasimTarihi { get; set; }
        public string? IcerikOzeti { get; set; }
        public string? IlgiliKisiKurum { get; set; }
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? EkranGoruntusu { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
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

        [StringLength(100)]
        public string? Tur { get; set; }

        [StringLength(2000)]
        public string? Aciklama { get; set; }

        [StringLength(50)]
        public string? Telefon { get; set; }

        [StringLength(250)]
        [EmailAddress]
        public string? Eposta { get; set; }

        [StringLength(2000)]
        public string? SosyalMedyaHesaplari { get; set; }

        [StringLength(100)]
        public string? SiyasiYonelim { get; set; }

        [StringLength(100)]
        public string? KutukNumarasi { get; set; }

        public Guid? UstKurulusId { get; set; }

        /// <summary>Kuruluş logosu (base64 encoded resim).</summary>
        public string? Logo { get; set; }
    }

    public class OrganizatorResponseDto
    {
        public Guid Id { get; set; }
        public string? Ad { get; set; }
        public DateTime KurulusTarihi { get; set; }
        public string? FaaliyetAlani { get; set; }
        public string? Iletisim { get; set; }
        public string? Tur { get; set; }
        public string? Aciklama { get; set; }
        public string? Telefon { get; set; }
        public string? Eposta { get; set; }
        public string? SosyalMedyaHesaplari { get; set; }
        public string? SiyasiYonelim { get; set; }
        public string? KutukNumarasi { get; set; }
        public Guid? UstKurulusId { get; set; }
        public string? UstKurulusAd { get; set; }
        public int AltKurulusSayisi { get; set; }
        public string? Logo { get; set; }
    }

    // ── Konu ─────────────────────────────────────────────────────────────
    public class KonuCreateDto
    {
        [Required(ErrorMessage = "Konu ad\u0131 zorunludur.")]
        [StringLength(250)]
        public string? Ad { get; set; }

        [StringLength(2000)]
        public string? Aciklama { get; set; }

        [StringLength(100)]
        public string? Tur { get; set; }

        public Guid? UstKonuId { get; set; }
    }

    public class KonuResponseDto
    {
        public Guid Id { get; set; }
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
        public string? Tur { get; set; }
        public Guid? UstKonuId { get; set; }
        public string? UstKonuAd { get; set; }
        public int AltKonuSayisi { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Secim ────────────────────────────────────────────────────────────
    public class SandikOlayCreateDto
    {
        [Required(ErrorMessage = "Musahit adi zorunludur.")]
        [MinLength(3, ErrorMessage = "Musahit adi en az 3 karakter olmalidir.")]
        [StringLength(250)]
        public string? MusahitAdi { get; set; }

        [Required(ErrorMessage = "Il zorunludur.")]
        [StringLength(100)]
        public string? Il { get; set; }

        [Required(ErrorMessage = "Ilce zorunludur.")]
        [StringLength(100)]
        public string? Ilce { get; set; }

        [StringLength(100)]
        public string? Mahalle { get; set; }

        [StringLength(200)]
        public string? Okul { get; set; }

        [StringLength(250)]
        public string? Konu { get; set; }

        [Range(1, 99999, ErrorMessage = "Gecerli bir sandik numarasi giriniz.")]
        public int SandikNo { get; set; }

        [Required(ErrorMessage = "Olay kategorisi zorunludur.")]
        [StringLength(100)]
        public string? OlayKategorisi { get; set; }

        public TimeSpan OlaySaati { get; set; }

        [Required(ErrorMessage = "Aciklama zorunludur.")]
        [StringLength(2000)]
        public string? Aciklama { get; set; }

        public string? KanitDosyasi { get; set; }

        public DateTime Tarih { get; set; }

        [Range(0, int.MaxValue)]
        public int KatilimciSayisi { get; set; }

        [Range(0, int.MaxValue)]
        public int SehitSayisi { get; set; }

        [Range(0, int.MaxValue)]
        public int OluSayisi { get; set; }

        [Range(0, int.MaxValue)]
        public int GozaltiSayisi { get; set; }
    }

    public class SandikOlayResponseDto
    {
        public Guid Id { get; set; }
        public string? MusahitAdi { get; set; }
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? Mahalle { get; set; }
        public string? Okul { get; set; }
        public string? Konu { get; set; }
        public int SandikNo { get; set; }
        public string? OlayKategorisi { get; set; }
        public TimeSpan OlaySaati { get; set; }
        public string? Aciklama { get; set; }
        public string? KanitDosyasi { get; set; }
        public DateTime Tarih { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public int KatilimciSayisi { get; set; }
        public int SehitSayisi { get; set; }
        public int OluSayisi { get; set; }
        public int GozaltiSayisi { get; set; }
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

        public ZiyaretDurumu ZiyaretDurumu { get; set; } = ZiyaretDurumu.Planlandi;

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
        public ZiyaretDurumu ZiyaretDurumu { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
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
        public string GSM { get; set; } = string.Empty;        public int? CityId { get; set; }
        public string Birim { get; set; } = string.Empty;    }
    /// <summary>Rol atama işlemi için request body DTO'su.</summary>
    public class RolAtamaDto
    {
        public string YeniRol { get; set; } = string.Empty;
        public int? CityId { get; set; }
    }

    /// <summary>Kullanıcı bilgilerini güncelleme için DTO.</summary>
    public class UserGuncelleDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
        public string Birim { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? CityId { get; set; }
    }

    // ── Veri Girişi Listesi ──────────────────────────────────────────────
    public class VeriGirisiDto
    {
        public int Sicil { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string Birim { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public string Konu { get; set; } = string.Empty;
        public string Faaliyet { get; set; } = string.Empty;
    }
}
