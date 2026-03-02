using System;
using System.Collections.Generic;
using EGM.Domain.Enums;

namespace EGM.Domain.Entities
{
    public class Olay
    {
        public int Id { get; set; }

        public string? Baslik { get; set; }
        public string? OlayTuru { get; set; }

        public int OrganizatorId { get; set; }
        public Organizator? Organizator { get; set; }

        public int KonuId { get; set; }
        public Konu? Konu { get; set; }

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

        public OlayDurum Durum { get; set; }
        public Hassasiyet Hassasiyet { get; set; }
        public double RiskPuani { get; set; }

        public ICollection<YuruyusRota> YuruyusRotasi { get; set; } = new List<YuruyusRota>();

        public ICollection<OperasyonelFaaliyet> OperasyonelFaaliyetler { get; set; } = new List<OperasyonelFaaliyet>();
        public ICollection<SosyalMedyaOlay> SosyalMedyaOlaylar { get; set; } = new List<SosyalMedyaOlay>();
    }
}

