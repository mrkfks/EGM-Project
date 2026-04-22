using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class Olay : BaseEntity
    {
        /// <summary>
        /// Olay Numarası (yyyy.aa.gg.İlPlaka.Sıra)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string OlayNo { get; set; } = string.Empty;

        public OlayDurum Durum { get; set; }

        [Required]
        public DateTime BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }

        // İlişkiler (FK)
        [Required]
        public Guid TurId { get; set; }
        public OlayTuru? Tur { get; set; }

        [Required]
        public Guid SekilId { get; set; }
        public GerceklesmeSekli? Sekil { get; set; }

        [Required]
        public Guid KonuId { get; set; }
        public Konu? Konu { get; set; }

        [Required]
        public Guid OrganizatorId { get; set; }
        public Organizator? Organizator { get; set; }

        public string? Aciklama { get; set; }

        [Required]
        public Guid PersonelId { get; set; }
        public User? Personel { get; set; }

        // Navigasyonlar (1:N, 1:1, N:N)
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public ICollection<Location> Locations { get; set; } = new List<Location>();
        public EventDetail? EventDetail { get; set; }
        public ICollection<Organizator> ParticipantOrganizators { get; set; } = new List<Organizator>();

        /// <summary>
        /// Arka plan servisi için bildirim kontrolü.
        /// </summary>
        public bool BaslangicBildirimiGonderildi { get; set; } = false;
        
        /// <summary>
        /// Olayın asıl kayıt ili (Plaka kodu için).
        /// </summary>
        public int? CityId { get; set; }
    }
}


