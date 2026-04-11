import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl + '/api';

const HASSASIYET_LABEL: Record<number, string> = {
  0: 'Düşük', 1: 'Orta', 2: 'Yüksek', 3: 'Kritik'
};

interface SosyalMedyaItem {
  id: string;
  platform?: string;
  konu?: string;
  paylasimLinki?: string;
  paylasimTarihi?: string;
  icerikOzeti?: string;
  ilgiliKisiKurum?: string;
  il?: string;
  ilce?: string;
  hassasiyet: number;
  takipNo?: string;
}

@Component({
  selector: 'app-sosyal-medya-istatistik',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sosyal-medya-istatistik.html',
  styleUrl:    './sosyal-medya-istatistik.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SosyalMedyaIstatistik implements OnInit {

  tumKayitlar: SosyalMedyaItem[] = [];
  filtreli:    SosyalMedyaItem[] = [];
  yukleniyor = false;
  hata: string | null = null;

  // Filtreler
  filtreHassasiyet    = '';
  filtreIl            = '';
  filtreIlce          = '';
  filtrePlatform      = '';
  filtreKonu          = '';
  filtreIlgiliKisi    = '';
  filtreTarih1        = '';
  filtreTarih2        = '';

  // Seçenek listeleri
  tumPlatformlar: string[] = [];
  tumKonular: string[] = [];
  tumKisilerKurumlar: string[] = [];

  // Sayfalama
  readonly sayfaBoyutu = 20;
  mevcutSayfa = 1;

  readonly hassasiyetLabel = HASSASIYET_LABEL;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.yukle(); }

  yukle(): void {
    this.yukleniyor = true;
    this.hata = null;
    this.http.get<SosyalMedyaItem[]>(`${API}/sosyalmedyaolay`).subscribe({
      next: res => {
        this.tumKayitlar = res;
        this.secenekleriCekil();
        this.filtrele();
        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.hata = 'Veriler yüklenirken hata oluştu.';
        this.yukleniyor = false;
        this.cdr.markForCheck();
      }
    });
  }

  secenekleriCekil(): void {
    this.tumPlatformlar = [...new Set(this.tumKayitlar.map(o => o.platform ?? '').filter(v => v))].sort();
    this.tumKonular = [...new Set(this.tumKayitlar.map(o => o.konu ?? '').filter(v => v))].sort();
    this.tumKisilerKurumlar = [...new Set(this.tumKayitlar.map(o => o.ilgiliKisiKurum ?? '').filter(v => v))].sort();
  }

  filtrele(): void {
    this.mevcutSayfa = 1;
    let sonuc = [...this.tumKayitlar];

    if (this.filtreHassasiyet !== '')
      sonuc = sonuc.filter(s => s.hassasiyet === +this.filtreHassasiyet);
    if (this.filtrePlatform)
      sonuc = sonuc.filter(s => s.platform === this.filtrePlatform);
    if (this.filtreKonu)
      sonuc = sonuc.filter(s => s.konu === this.filtreKonu);
    if (this.filtreIlgiliKisi)
      sonuc = sonuc.filter(s => s.ilgiliKisiKurum === this.filtreIlgiliKisi);
    if (this.filtreIl)
      sonuc = sonuc.filter(s => s.il === this.filtreIl);
    if (this.filtreIlce)
      sonuc = sonuc.filter(s => s.ilce === this.filtreIlce);
    if (this.filtreTarih1) {
      const t1 = new Date(this.filtreTarih1);
      sonuc = sonuc.filter(s => s.paylasimTarihi && new Date(s.paylasimTarihi) >= t1);
    }
    if (this.filtreTarih2) {
      const t2 = new Date(this.filtreTarih2);
      t2.setHours(23, 59, 59);
      sonuc = sonuc.filter(s => s.paylasimTarihi && new Date(s.paylasimTarihi) <= t2);
    }
    this.filtreli = sonuc;
  }

  filtreleriSifirla(): void {
    this.filtreHassasiyet = this.filtrePlatform = this.filtreKonu = '';
    this.filtreIlgiliKisi = this.filtreIl = this.filtreIlce = '';
    this.filtreTarih1 = this.filtreTarih2 = '';
    this.filtrele();
  }

  get toplamSayfa(): number {
    return Math.max(1, Math.ceil(this.filtreli.length / this.sayfaBoyutu));
  }

  get sayfaKayitlari(): SosyalMedyaItem[] {
    const start = (this.mevcutSayfa - 1) * this.sayfaBoyutu;
    return this.filtreli.slice(start, start + this.sayfaBoyutu);
  }

  oncekiSayfa(): void { if (this.mevcutSayfa > 1) this.mevcutSayfa--; }
  sonrakiSayfa(): void { if (this.mevcutSayfa < this.toplamSayfa) this.mevcutSayfa++; }

  sayfayaGit(p: number): void {
    if (p >= 1 && p <= this.toplamSayfa) this.mevcutSayfa = p;
  }

  get ilSecenekler(): string[] {
    return [...new Set(this.tumKayitlar.map(o => o.il ?? '').filter(v => v))].sort();
  }

  get ilceSecenekler(): string[] {
    const base = this.filtreIl ? this.tumKayitlar.filter(o => o.il === this.filtreIl) : this.tumKayitlar;
    return [...new Set(base.map(o => o.ilce ?? '').filter(v => v))].sort();
  }

  get sayfaNumaralari(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.mevcutSayfa - 2);
    const end   = Math.min(this.toplamSayfa, this.mevcutSayfa + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  hassasiyetClass(h: number): string {
    return ['hass-dusuk', 'hass-orta', 'hass-yuksek', 'hass-kritik'][h] ?? '';
  }
}
