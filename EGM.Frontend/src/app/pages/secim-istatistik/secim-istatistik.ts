import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl + '/api';

interface SandikOlayItem {
  id: string;
  musahitAdi?: string;
  il?: string;
  ilce?: string;
  mahalle?: string;
  okul?: string;
  konu?: string;
  sandikNo?: string;
  olayKategorisi?: string;
  olaySaati?: string;
  aciklama?: string;
  tarih?: string;
  katilimciSayisi?: number;
  sehitSayisi?: number;
  oluSayisi?: number;
  gozaltiSayisi?: number;
  takipNo?: string;
}

@Component({
  selector: 'app-secim-istatistik',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './secim-istatistik.html',
  styleUrl:    './secim-istatistik.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SecimIstatistik implements OnInit {

  tumKayitlar: SandikOlayItem[] = [];
  filtreli:    SandikOlayItem[] = [];
  yukleniyor = false;
  hata: string | null = null;

  // Filtreler
  filtreMusahit    = '';
  filtreIl         = '';
  filtreIlce       = '';
  filtreMahalle    = '';
  filtreOkul       = '';
  filtreSandikNo   = '';
  filtreKonu       = '';
  filtreKategori   = '';
  filtreTarih1     = '';
  filtreTarih2     = '';

  // Seçenek listeleri
  tumMusahitler: string[] = [];
  tumMahalleler: string[] = [];
  tumOkullar: string[] = [];
  tumSandikNolari: string[] = [];
  tumKonular: string[] = [];
  tumKategoriler: string[] = [];

  // Sayfalama
  readonly sayfaBoyutu = 20;
  mevcutSayfa = 1;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.yukle(); }

  yukle(): void {
    this.yukleniyor = true;
    this.hata = null;
    this.http.get<SandikOlayItem[]>(`${API}/secim/sandik-olay`).subscribe({
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
    this.tumMusahitler = [...new Set(this.tumKayitlar.map(o => o.musahitAdi ?? '').filter(v => v))].sort();
    this.tumMahalleler = [...new Set(this.tumKayitlar.map(o => o.mahalle ?? '').filter(v => v))].sort();
    this.tumOkullar = [...new Set(this.tumKayitlar.map(o => o.okul ?? '').filter(v => v))].sort();
    this.tumSandikNolari = [...new Set(this.tumKayitlar.map(o => o.sandikNo ?? '').filter(v => v))].sort();
    this.tumKonular = [...new Set(this.tumKayitlar.map(o => o.konu ?? '').filter(v => v))].sort();
    this.tumKategoriler = [...new Set(this.tumKayitlar.map(o => o.olayKategorisi ?? '').filter(v => v))].sort();
  }

  filtrele(): void {
    this.mevcutSayfa = 1;
    let sonuc = [...this.tumKayitlar];

    if (this.filtreMusahit)
      sonuc = sonuc.filter(s => s.musahitAdi === this.filtreMusahit);
    if (this.filtreIl)
      sonuc = sonuc.filter(s => s.il === this.filtreIl);
    if (this.filtreIlce)
      sonuc = sonuc.filter(s => s.ilce === this.filtreIlce);
    if (this.filtreMahalle)
      sonuc = sonuc.filter(s => s.mahalle === this.filtreMahalle);
    if (this.filtreOkul)
      sonuc = sonuc.filter(s => s.okul === this.filtreOkul);
    if (this.filtreSandikNo)
      sonuc = sonuc.filter(s => s.sandikNo === this.filtreSandikNo);
    if (this.filtreKonu)
      sonuc = sonuc.filter(s => s.konu === this.filtreKonu);
    if (this.filtreKategori)
      sonuc = sonuc.filter(s => s.olayKategorisi === this.filtreKategori);
    if (this.filtreTarih1) {
      const t1 = new Date(this.filtreTarih1);
      sonuc = sonuc.filter(s => s.tarih && new Date(s.tarih) >= t1);
    }
    if (this.filtreTarih2) {
      const t2 = new Date(this.filtreTarih2);
      t2.setHours(23, 59, 59);
      sonuc = sonuc.filter(s => s.tarih && new Date(s.tarih) <= t2);
    }
    this.filtreli = sonuc;
  }

  filtreleriSifirla(): void {
    this.filtreMusahit = this.filtreIl = this.filtreIlce = this.filtreMahalle = '';
    this.filtreOkul = this.filtreSandikNo = this.filtreKonu = this.filtreKategori = '';
    this.filtreTarih1 = this.filtreTarih2 = '';
    this.filtrele();
  }

  get toplamSayfa(): number {
    return Math.max(1, Math.ceil(this.filtreli.length / this.sayfaBoyutu));
  }

  get sayfaKayitlari(): SandikOlayItem[] {
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
}
