import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl;

interface KonuKaydi {
  id: string;
  ad: string;
  aciklama: string | null;
  tur: string | null;
  ustKonuId: string | null;
  ustKonuAd: string | null;
  altKonuSayisi: number;
  createdAt: string;
}

interface OlayOzet {
  id: string;
  baslik: string;
  olayTuru: string;
  tarih: string;
  il: string;
  ilce: string | null;
  mekan: string | null;
  katilimciSayisi: number | null;
  gozaltiSayisi: number | null;
  sehitOluSayisi: number | null;
  durum: number;
  riskPuani: number;
  hassasiyet: number;
  organizatorAd: string | null;
  konuAd: string | null;
  aciklama: string | null;
  evrakNumarasi: string | null;
}

@Component({
  selector: 'app-konu-detay',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './konu-detay.html',
  styleUrls: ['./konu-detay.css'],
})
export class KonuDetay implements OnInit {
  konuId: string | null = null;
  konu: KonuKaydi | null = null;
  olaylar: OlayOzet[] = [];
  filtreliOlaylar: OlayOzet[] = [];

  yukleniyor = true;
  olayYukleniyor = false;
  hataMesaji = '';

  aramaMetni = '';
  turFiltresi = '';
  durumFiltresi = '';

  readonly durumEtiketleri: Record<number, string> = {
    0: 'Planlandı', 1: 'Devam Ediyor', 2: 'Tamamlandı', 3: 'İptal'
  };

  readonly durumRenkleri: Record<number, { bg: string; color: string }> = {
    0: { bg: '#fef9e7', color: '#b7950b' },
    1: { bg: '#ebf5fb', color: '#2980b9' },
    2: { bg: '#eafaf1', color: '#1e8449' },
    3: { bg: '#fdecea', color: '#c0392b' },
  };

  readonly hassasiyetEtiketleri: Record<number, string> = {
    0: 'Düşük', 1: 'Orta', 2: 'Yüksek', 3: 'Kritik'
  };

  readonly hassasiyetRenkleri: Record<number, { bg: string; color: string }> = {
    0: { bg: '#eafaf1', color: '#1e8449' },
    1: { bg: '#fef9e7', color: '#b7950b' },
    2: { bg: '#fdf2e9', color: '#ca6f1e' },
    3: { bg: '#fdecea', color: '#c0392b' },
  };

  readonly turRenkleri: Record<string, { bg: string; color: string; border: string }> = {
    'Ana Konu':                  { bg: '#eaf2ff', color: '#1a5276', border: '#a9cce3' },
    'Ekonomik ve Sosyal Haklar': { bg: '#fef9e7', color: '#b7950b', border: '#f9e79f' },
    'Isci Haklari Eylemleri':    { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' },
    'Emekli Haklari Eylemleri':  { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' },
    'Tarim ve Gida':             { bg: '#eafaf1', color: '#1e8449', border: '#a9dfbf' },
    'Cevre ve Iklim':            { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' },
    'Egitim':                    { bg: '#fdecea', color: '#c0392b', border: '#f1948a' },
    'Saglik':                    { bg: '#fdf2e9', color: '#ca6f1e', border: '#fad7a0' },
    'Diger':                     { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' },
  };

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      this.konuId = params.get('id');
      if (this.konuId) {
        this.konuYukle();
        this.olaylariYukle();
      }
    });
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  konuYukle(): void {
    this.yukleniyor = true;
    this.http.get<KonuKaydi>(`${API}/api/organizator/konu/${this.konuId}`, { headers: this.getHeaders() })
      .subscribe({
        next: (k) => { this.konu = k; this.yukleniyor = false; },
        error: () => { this.hataMesaji = 'Konu bilgisi yüklenemedi.'; this.yukleniyor = false; }
      });
  }

  olaylariYukle(): void {
    this.olayYukleniyor = true;
    this.http.get<OlayOzet[]>(`${API}/api/olay/by-konu/${this.konuId}`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => {
          this.olaylar = data;
          this.filtrele();
          this.olayYukleniyor = false;
        },
        error: () => { this.olayYukleniyor = false; }
      });
  }

  filtrele(): void {
    const ara = this.aramaMetni.toLowerCase().trim();
    this.filtreliOlaylar = this.olaylar.filter(o => {
      const metinUygun = !ara ||
        o.baslik.toLowerCase().includes(ara) ||
        (o.il ?? '').toLowerCase().includes(ara) ||
        (o.organizatorAd ?? '').toLowerCase().includes(ara);
      const turUygun = !this.turFiltresi || o.olayTuru === this.turFiltresi;
      const durumUygun = !this.durumFiltresi || o.durum === +this.durumFiltresi;
      return metinUygun && turUygun && durumUygun;
    });
  }

  turRenk(tur: string | null) {
    return this.turRenkleri[tur ?? 'Diger'] ?? this.turRenkleri['Diger'];
  }

  geri(): void {
    this.router.navigate(['/rapor-konular'], {
      queryParams: { ustKonuId: this.konu?.ustKonuId }
    });
  }

  get olayTurleri(): string[] {
    return [...new Set(this.olaylar.map(o => o.olayTuru).filter(Boolean))];
  }

  get toplamKatilimci(): number {
    return this.filtreliOlaylar.reduce((s, o) => s + (o.katilimciSayisi ?? 0), 0);
  }

  get toplamGozalti(): number {
    return this.filtreliOlaylar.reduce((s, o) => s + (o.gozaltiSayisi ?? 0), 0);
  }
}
