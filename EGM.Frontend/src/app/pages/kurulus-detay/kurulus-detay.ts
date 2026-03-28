import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../environments/environment';

interface KurulusKaydi {
  id: string;
  ad: string;
  kurulusTarihi: string;
  faaliyetAlani: string;
  iletisim: string;
  tur: string;
  aciklama: string;
  ustKurulusId: string | null;
  ustKurulusAd: string | null;
  altKurulusSayisi: number;
  siyasiYonelim?: string;
  kutukNumarasi?: string;
  telefon?: string;
  eposta?: string;
  sosyalMedyaHesaplari?: string;
}

interface OlayOzet {
  id: string;
  baslik: string;
  olayTuru: string;
  tarih: string;
  il: string;
  ilce: string;
  mekan: string;
  katilimciSayisi: number | null;
  gozaltiSayisi: number | null;
  durum: number;
  riskPuani: number;
  konuAd: string | null;
}

@Component({
  selector: 'app-kurulus-detay',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './kurulus-detay.html',
  styleUrls: ['./kurulus-detay.css'],
})
export class KurulusDetay implements OnInit {
  private readonly apiBase = `${environment.apiUrl}/api/organizator`;
  private readonly olayApiBase = `${environment.apiUrl}/api/olay`;

  kurulus: KurulusKaydi | null = null;
  altKuruluslar: KurulusKaydi[] = [];
  yukleniyor = true;
  hataMesaji = '';

  olaylar: OlayOzet[] = [];
  olayYukleniyor = false;
  olayTarihBaslangic = '';
  olayTarihBitis = '';
  olayArama = '';
  olayTurFiltre = '';
  olayDurumFiltre = '';
  olayRiskFiltre = '';
  olayIlFiltre = '';
  aktifSekme: 'bilgi' | 'olaylar' = 'bilgi';

  orgLogolari: Record<string, string> = {};
  sosyalMedya: { platform: string; hesap: string }[] = [];

  readonly turRenkleri: Record<string, { bg: string; color: string; border: string }> = {
    'Konfederasyon': { bg: '#fef9e7', color: '#d68910', border: '#f9e79f' },
    'Sendika':       { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' },
    'Siyasi Parti':  { bg: '#fdecea', color: '#c0392b', border: '#f1948a' },
    'Dernek':        { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' },
    'Vakif':         { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' },
    'STK':           { bg: '#eafaf1', color: '#1e8449', border: '#a9dfbf' },
    'Kamu Kurumu':   { bg: '#eaf2ff', color: '#1a5276', border: '#a9cce3' },
    'Diger':         { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' },
  };

  readonly durumEtiketleri: Record<number, { ad: string; bg: string; color: string }> = {
    0: { ad: 'Planlandı', bg: '#fff3cd', color: '#856404' },
    1: { ad: 'Devam Ediyor', bg: '#d1ecf1', color: '#0c5460' },
    2: { ad: 'Tamamlandı', bg: '#d4edda', color: '#155724' },
    3: { ad: 'İptal Edildi', bg: '#f8d7da', color: '#721c24' },
  };

  constructor(
    private http: HttpClient,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    const kayitli = localStorage.getItem('egm_org_logolar');
    if (kayitli) {
      try { this.orgLogolari = JSON.parse(kayitli); } catch { this.orgLogolari = {}; }
    }
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) this.kurulusuGetir(id);
    });
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  kurulusuGetir(id: string): void {
    this.yukleniyor = true;
    this.hataMesaji = '';
    this.http.get<KurulusKaydi[]>(this.apiBase, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        const bulunan = data.find(k => k.id === id);
        if (!bulunan) {
          this.hataMesaji = 'Kuruluş bulunamadı.';
          this.yukleniyor = false;
          return;
        }
        this.kurulus = bulunan;
        this.altKuruluslar = data.filter(k => k.ustKurulusId === id);
        if (bulunan.sosyalMedyaHesaplari) {
          try { this.sosyalMedya = JSON.parse(bulunan.sosyalMedyaHesaplari); } catch { this.sosyalMedya = []; }
        }
        this.yukleniyor = false;
      },
      error: () => {
        this.hataMesaji = 'Kuruluş bilgileri yüklenemedi.';
        this.yukleniyor = false;
      },
    });
  }

  sekmeGec(sekme: 'bilgi' | 'olaylar'): void {
    this.aktifSekme = sekme;
    if (sekme === 'olaylar' && this.olaylar.length === 0) {
      this.olaylariGetir();
    }
  }

  olaylariGetir(): void {
    if (!this.kurulus) return;
    this.olayYukleniyor = true;
    let params = new HttpParams();
    if (this.olayTarihBaslangic) params = params.set('tarihBaslangic', this.olayTarihBaslangic);
    if (this.olayTarihBitis)     params = params.set('tarihBitis', this.olayTarihBitis);
    this.http.get<OlayOzet[]>(
      `${this.olayApiBase}/by-organizator/${this.kurulus.id}`,
      { headers: this.getHeaders(), params }
    ).subscribe({
      next: (data) => { this.olaylar = data; this.olayYukleniyor = false; },
      error: () => { this.olayYukleniyor = false; },
    });
  }

  get filtrelenmisOlaylar(): OlayOzet[] {
    let liste = this.olaylar;
    const ara = this.olayArama.toLowerCase().trim();
    if (ara) liste = liste.filter(o => o.baslik.toLowerCase().includes(ara) || (o.il ?? '').toLowerCase().includes(ara));
    if (this.olayTurFiltre)  liste = liste.filter(o => o.olayTuru === this.olayTurFiltre);
    if (this.olayDurumFiltre !== '') liste = liste.filter(o => o.durum === +this.olayDurumFiltre);
    if (this.olayIlFiltre)   liste = liste.filter(o => o.il === this.olayIlFiltre);
    if (this.olayRiskFiltre === 'dusuk')   liste = liste.filter(o => o.riskPuani < 4);
    if (this.olayRiskFiltre === 'orta')    liste = liste.filter(o => o.riskPuani >= 4 && o.riskPuani < 7);
    if (this.olayRiskFiltre === 'yuksek')  liste = liste.filter(o => o.riskPuani >= 7);
    return liste;
  }

  get mevcutOlayTurleri(): string[] {
    return [...new Set(this.olaylar.map(o => o.olayTuru).filter(Boolean))].sort();
  }

  get mevcutIller(): string[] {
    return [...new Set(this.olaylar.map(o => o.il).filter(Boolean))].sort();
  }

  filtreleriTemizle(): void {
    this.olayArama = '';
    this.olayTurFiltre = '';
    this.olayDurumFiltre = '';
    this.olayRiskFiltre = '';
    this.olayIlFiltre = '';
    this.olayTarihBaslangic = '';
    this.olayTarihBitis = '';
    this.olaylariGetir();
  }

  get aktifFiltreSayisi(): number {
    return [this.olayArama, this.olayTurFiltre, this.olayDurumFiltre, this.olayRiskFiltre, this.olayIlFiltre,
            this.olayTarihBaslangic, this.olayTarihBitis].filter(Boolean).length;
  }

  acikFotograf: string | null = null;
  acikFotografBaslik = '';

  fotografiAc(src: string, baslik: string): void {
    this.acikFotograf = src;
    this.acikFotografBaslik = baslik;
  }

  fotografiKapat(): void {
    this.acikFotograf = null;
    this.acikFotografBaslik = '';
  }

  geriDon(): void {
    this.router.navigate(['/rapor-kuruluslar']);
  }

  navigateToUst(id: string): void {
    this.router.navigate(['/kurulus-detay', id]);
  }

  navigateToAlt(id: string): void {
    this.router.navigate(['/kurulus-detay', id]);
  }

  turRenk(tur: string) {
    return this.turRenkleri[tur] ?? this.turRenkleri['Diger'];
  }

  tarihFormat(tarih: string): string {
    if (!tarih) return '-';
    try { return new Date(tarih).toLocaleDateString('tr-TR'); } catch { return tarih; }
  }
}
