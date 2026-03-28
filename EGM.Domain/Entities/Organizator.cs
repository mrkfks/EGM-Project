using System;
using System.Collections.Generic;

namespace EGM.Domain.Entities
{
    public class Organizator : BaseEntity
    {
        public string? Ad { get; set; }
        public DateTime KurulusTarihi { get; set; }
        public string? FaaliyetAlani { get; set; }
        public string? Iletisim { get; set; }

        /// <summary>Konfederasyon, Sendika, Vakif, Dernek, Siyasi Parti, STK, Diger</summary>
        public string? Tur { get; set; }

        /// <summary>Kisa aciklama / not</summary>
        public string? Aciklama { get; set; }

        public string? Telefon { get; set; }
        public string? Eposta { get; set; }

        /// <summary>JSON dizisi: [{"platform":"Twitter","hesap":"@ornek"}]</summary>
        public string? SosyalMedyaHesaplari { get; set; }

        /// <summary>Siyasi yonelim / ideoloji (Sol, Merkez, Sag, Liberal, Muhafazakar vb.)</summary>
        public string? SiyasiYonelim { get; set; }

        /// <summary>Resmi kutuk / sicil kayit numarasi</summary>
        public string? KutukNumarasi { get; set; }

        /// <summary>Ust kurulusun Id'si (hiyerarsi icin). Null ise ust duzeyde bagimsiz kurulustur.</summary>
        public Guid? UstKurulusId { get; set; }
        public Organizator? UstKurulus { get; set; }

        /// <summary>Bu kurulusun altindaki alt-kurluslar</summary>
        public ICollection<Organizator> AltKuruluslar { get; set; } = new List<Organizator>();

        public ICollection<Olay> Olaylar { get; set; } = new List<Olay>();
        public ICollection<KategoriOrganizator> Kategoriler { get; set; } = new List<KategoriOrganizator>();
    }
}
