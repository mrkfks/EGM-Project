import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
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
  olayTuru: string;
  tarih: string;
  il: string;
  ilce: string;
  mekan: string;
  katilimciSayisi: number | null;
  gozaltiSayisi: number | null;
  durum: number;
  konuAd: string | null;
}

@Component({
  selector: 'app-kurulus-detay',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './kurulus-detay.html',
  styleUrls: ['./kurulus-detay.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KurulusDetay implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
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
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const kayitli = localStorage.getItem('egm_org_logolar');
    if (kayitli) {
      try { this.orgLogolari = JSON.parse(kayitli); } catch { this.orgLogolari = {}; }
    }
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const id = params.get('id');
      if (id) this.kurulusuGetir(id);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  kurulusuGetir(id: string): void {
    this.yukleniyor = true;
    this.hataMesaji = '';

    // Kuruluş detayını ve alt kuruluşları paralel çek
    this.http.get<KurulusKaydi>(`${this.apiBase}/${id}`, { headers: this.getHeaders() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (kurulus) => {
          this.kurulus = kurulus;
          if (kurulus.sosyalMedyaHesaplari) {
            try { this.sosyalMedya = JSON.parse(kurulus.sosyalMedyaHesaplari); } catch { this.sosyalMedya = []; }
          }
          this.yukleniyor = false;
          this.cdr.markForCheck();
          // Alt kuruluşları ayrıca yükle
          this.altKuruluslariGetir(id);
        },
        error: () => {
          this.hataMesaji = 'Kuruluş bilgileri yüklenemedi.';
          this.yukleniyor = false;
          this.cdr.markForCheck();
        },
      });
  }

  private altKuruluslariGetir(ustId: string): void {
    this.http.get<KurulusKaydi[]>(`${this.apiBase}?ustKurulusId=${ustId}`, { headers: this.getHeaders() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.altKuruluslar = data;
          this.cdr.markForCheck();
        },
        error: () => { this.altKuruluslar = []; },
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
    ).pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.olaylar = data; this.olayYukleniyor = false; this.cdr.markForCheck(); },
      error: () => { this.olayYukleniyor = false; this.cdr.markForCheck(); },
    });
  }

  get filtrelenmisOlaylar(): OlayOzet[] {
    let liste = this.olaylar;
    const ara = this.olayArama.toLowerCase().trim();
    if (ara) liste = liste.filter(o => (o.il ?? '').toLowerCase().includes(ara));
    if (this.olayTurFiltre)  liste = liste.filter(o => o.olayTuru === this.olayTurFiltre);
    if (this.olayDurumFiltre !== '') liste = liste.filter(o => o.durum === +this.olayDurumFiltre);
    if (this.olayIlFiltre)   liste = liste.filter(o => o.il === this.olayIlFiltre);
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
    this.olayIlFiltre = '';
    this.olayTarihBaslangic = '';
    this.olayTarihBitis = '';
    this.olaylariGetir();
  }

  get aktifFiltreSayisi(): number {
    return [this.olayArama, this.olayTurFiltre, this.olayDurumFiltre, this.olayIlFiltre,
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
