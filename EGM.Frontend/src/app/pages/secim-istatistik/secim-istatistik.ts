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

  filtrele(): void {
    this.mevcutSayfa = 1;
    let sonuc = [...this.tumKayitlar];

    if (this.filtreMusahit.trim())
      sonuc = sonuc.filter(s => s.musahitAdi?.toLowerCase().includes(this.filtreMusahit.trim().toLowerCase()));
    if (this.filtreIl.trim())
      sonuc = sonuc.filter(s => s.il?.toLowerCase().includes(this.filtreIl.trim().toLowerCase()));
    if (this.filtreIlce.trim())
      sonuc = sonuc.filter(s => s.ilce?.toLowerCase().includes(this.filtreIlce.trim().toLowerCase()));
    if (this.filtreMahalle.trim())
      sonuc = sonuc.filter(s => s.mahalle?.toLowerCase().includes(this.filtreMahalle.trim().toLowerCase()));
    if (this.filtreOkul.trim())
      sonuc = sonuc.filter(s => s.okul?.toLowerCase().includes(this.filtreOkul.trim().toLowerCase()));
    if (this.filtreSandikNo.trim())
      sonuc = sonuc.filter(s => s.sandikNo?.toLowerCase().includes(this.filtreSandikNo.trim().toLowerCase()));
    if (this.filtreKonu.trim())
      sonuc = sonuc.filter(s => s.konu?.toLowerCase().includes(this.filtreKonu.trim().toLowerCase()));
    if (this.filtreKategori.trim())
      sonuc = sonuc.filter(s => s.olayKategorisi?.toLowerCase().includes(this.filtreKategori.trim().toLowerCase()));
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

  get sayfaNumaralari(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.mevcutSayfa - 2);
    const end   = Math.min(this.toplamSayfa, this.mevcutSayfa + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }
}
