using System;
using EGM.Domain.Enums;

namespace EGM.Application.DTOs
{
    // ── Olay ────────────────────────────────────────────────────────────
    public class OlayCreateDto
    {
        public string? Baslik { get; set; }
        public string? OlayTuru { get; set; }
        public Guid OrganizatorId { get; set; }
        public Guid KonuId { get; set; }
        public DateTime Tarih { get; set; }
        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }
        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? Mekan { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? KatilimciSayisi { get; set; }
        public string? Aciklama { get; set; }
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
    }

    // ── OperasyonelFaaliyet ──────────────────────────────────────────────
    public class OperasyonelFaaliyetCreateDto
    {
        public Guid OlayId { get; set; }
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
        public string? GrupAdi { get; set; }
        public int KatilimciSayisi { get; set; }
    }

    // ── Supheli ─────────────────────────────────────────────────────────
    public class SupheliCreateDto
    {
        public Guid OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? TcKimlikNo { get; set; }
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
        public Guid OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? TcKimlikNo { get; set; }
        public DateTime DogumTarihi { get; set; }
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
        public Guid OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? TcKimlikNo { get; set; }
        public DateTime DogumTarihi { get; set; }
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
        public Guid OlayId { get; set; }
        public string? Platform { get; set; }
        public string? PaylasimLinki { get; set; }
        public DateTime PaylasimTarihi { get; set; }
        public string? IcerikOzeti { get; set; }
        public string? IlgiliKisiKurum { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
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
        public string? Ad { get; set; }
        public DateTime KurulusTarihi { get; set; }
        public string? FaaliyetAlani { get; set; }
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
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
    }

    // ── Secim ────────────────────────────────────────────────────────────
    public class SecimSonucuCreateDto
    {
        public string? SecimTuru { get; set; }
        public DateTime Tarih { get; set; }
        public string? BolgeTipi { get; set; }
        public int BolgeId { get; set; }
        public Guid AdayId { get; set; }
        public Guid PartiId { get; set; }
        public int OySayisi { get; set; }
        public double OyOrani { get; set; }
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
        public string? AdSoyad { get; set; }
        public string? PartiAdi { get; set; }
    }

    public class PartiCreateDto
    {
        public string? Ad { get; set; }
    }

    // ── VIP Ziyaret ──────────────────────────────────────────────────────
    public class VIPZiyaretCreateDto
    {
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
        public Guid VIPZiyaretId { get; set; }
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
    }

    public class EkipCreateDto
    {
        public Guid VIPZiyaretId { get; set; }
        public string? Ad { get; set; }
    }

    // ── Yuruyu Rotasi ───────────────────────────────────────────────────
    public class RotaNoktasiCreateDto
    {
        public string? NoktaAdi { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
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
