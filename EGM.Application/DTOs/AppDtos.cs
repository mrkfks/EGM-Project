using System;
using EGM.Domain.Enums;

namespace EGM.Application.DTOs
{
    // ── Olay ────────────────────────────────────────────────────────────
    public class OlayCreateDto
    {
        public string? Baslik { get; set; }
        public string? OlayTuru { get; set; }
        public int OrganizatorId { get; set; }
        public int KonuId { get; set; }
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
    }

    public class OlayResponseDto
    {
        public int Id { get; set; }
        public string? Baslik { get; set; }
        public string? OlayTuru { get; set; }
        public int OrganizatorId { get; set; }
        public int KonuId { get; set; }
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

    // ── Kümeleme (Clustering) ────────────────────────────────────────────
    public class OlayClusterDto
    {
        /// <summary>Kümenin merkez enlemi</summary>
        public double Lat { get; set; }
        /// <summary>Kümenin merkez boylamı</summary>
        public double Lon { get; set; }
        /// <summary>Bu hücredeki olay sayısı</summary>
        public int Count { get; set; }
    }

    // ── OperasyonelFaaliyet ──────────────────────────────────────────────
    public class OperasyonelFaaliyetCreateDto
    {
        public int OlayId { get; set; }
        public string? Aciklama { get; set; }
    }

    public class OperasyonelFaaliyetResponseDto
    {
        public int Id { get; set; }
        public int OlayId { get; set; }
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

    // ── Şüpheli ─────────────────────────────────────────────────────────
    public class SupheliCreateDto
    {
        public int OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? TcKimlikNo { get; set; }
        public DateTime DogumTarihi { get; set; }
        public bool Gozaltinda { get; set; }
    }

    public class SupheliResponseDto
    {
        public int Id { get; set; }
        public int OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public bool Gozaltinda { get; set; }
        // TC Kimlik No kasıtlı olarak response'a dahil edilmedi (gizlilik)
    }

    // ── Şehit ───────────────────────────────────────────────────────────
    public class SehitCreateDto
    {
        public int OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? TcKimlikNo { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? Gorev { get; set; }
    }

    public class SehitResponseDto
    {
        public int Id { get; set; }
        public int OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? Gorev { get; set; }
    }

    // ── Ölü ─────────────────────────────────────────────────────────────
    public class OluCreateDto
    {
        public int OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? TcKimlikNo { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? KatilimciDurumu { get; set; }
    }

    public class OluResponseDto
    {
        public int Id { get; set; }
        public int OperasyonelFaaliyetId { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? KatilimciDurumu { get; set; }
    }

    // ── Sosyal Medya Olayı ───────────────────────────────────────────────
    public class SosyalMedyaOlayCreateDto
    {
        public int OlayId { get; set; }
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
        public int Id { get; set; }
        public int OlayId { get; set; }
        public string? Platform { get; set; }
        public string? PaylasimLinki { get; set; }
        public DateTime PaylasimTarihi { get; set; }
        public string? IcerikOzeti { get; set; }
        public string? IlgiliKisiKurum { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public double SosyalSignalSkoru { get; set; }
    }

    // ── Organizatör ──────────────────────────────────────────────────────
    public class OrganizatorCreateDto
    {
        public string? Ad { get; set; }
        public DateTime KurulusTarihi { get; set; }
        public string? FaaliyetAlani { get; set; }
        public string? Iletisim { get; set; }
    }

    public class OrganizatorResponseDto
    {
        public int Id { get; set; }
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

    // ── Seçim ────────────────────────────────────────────────────────────
    public class SecimSonucuCreateDto
    {
        public string? SecimTuru { get; set; }
        public DateTime Tarih { get; set; }
        public string? BolgeTipi { get; set; }
        public int BolgeId { get; set; }
        public int AdayId { get; set; }
        public int PartiId { get; set; }
        public int OySayisi { get; set; }
        public double OyOrani { get; set; }
        public int KaynakId { get; set; }
        public bool KaynakOnayDurumu { get; set; }
    }

    public class SecimSonucuResponseDto
    {
        public int Id { get; set; }
        public string? SecimTuru { get; set; }
        public DateTime Tarih { get; set; }
        public string? BolgeTipi { get; set; }
        public int BolgeId { get; set; }
        public int AdayId { get; set; }
        public int PartiId { get; set; }
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
        public int Id { get; set; }
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
        public int VIPZiyaretId { get; set; }
        public string? Ad { get; set; }
        public string? Aciklama { get; set; }
    }

    public class EkipCreateDto
    {
        public int VIPZiyaretId { get; set; }
        public string? Ad { get; set; }
    }

    // ── Yürüyüş Rotası ───────────────────────────────────────────────────
    public class RotaNoktasiCreateDto
    {
        public string? NoktaAdi { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int SiraNo { get; set; }
    }

    // ── Kullanıcı ────────────────────────────────────────────────────────
    public class UserResponseDto
    {
        public int Id { get; set; }
        public int Sicil { get; set; }
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
    }
}
