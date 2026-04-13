using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class Olay : BaseEntity
    {
        [Required(ErrorMessage = "Olay türü zorunludur.")]
        [StringLength(100)]
        public string? OlayTuru { get; set; }

        [Required]
        public Guid OrganizatorId { get; set; }
        public Organizator? Organizator { get; set; }

        [Required]
        public Guid KonuId { get; set; }
        public Konu? Konu { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }
        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }

        public string? Il { get; set; }
        public string? Ilce { get; set; }
        public string? Mahalle { get; set; }
        public string? Mekan { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Katılımcı sayısı negatif olamaz.")]
        public int? KatilimciSayisi { get; set; }
        [Range(0, int.MaxValue)]
        public int? GozaltiSayisi { get; set; }
        [Range(0, int.MaxValue)]
        public int? SehitOluSayisi { get; set; }
        public string? Aciklama { get; set; }
        public string? EvrakNumarasi { get; set; }

        public OlayDurum Durum { get; set; }
        public Hassasiyet Hassasiyet { get; set; }

        /// <summary>Olayın gerçekleştiği ilin plaka kodu. CityId filtresi bu alan üzerinden çalışır.</summary>
        public int? CityId { get; set; }

        public DateTime? GercekBaslangicTarihi { get; set; }
        public DateTime? GercekBitisTarihi { get; set; }

        /// <summary>Formdan girilen planlanan bitiş tarihi (GercekBitisTarihi'nden farklı).</summary>
        public DateTime? OlayBitisTarihi { get; set; }

        /// <summary>Etkinlik sonunda gerçekleşen katılımcı sayısı (planlananından farklı olabilir).</summary>
        public int? GerceklesenKatilimciSayisi { get; set; }

        /// <summary>Gerçekleşme şekli kaydı (FK). Null ise belirtilmemiş.</summary>
        public Guid? GerceklesmeSekliId { get; set; }
        public GerceklesmeSekli? GerceklesmeSekli { get; set; }

        public ICollection<YuruyusRota> YuruyusRotasi { get; set; } = new List<YuruyusRota>();
        public ICollection<SosyalMedyaOlay> SosyalMedyaOlaylar { get; set; } = new List<SosyalMedyaOlay>();

        /// <summary>Otomatik üretilen benzersiz takip numarası. Format: SO-YYYYMMDDPP-SSS</summary>
        public string? TakipNo { get; set; }

        /// <summary>
        /// Olay başlangıç saati geldiğinde "Olay Başladı" bildirimi gönderildi mi?
        /// true ise arka plan servisi tekrar bildirim göndermez.
        /// </summary>
        public bool BaslangicBildirimiGonderildi { get; set; } = false;
    }
}

