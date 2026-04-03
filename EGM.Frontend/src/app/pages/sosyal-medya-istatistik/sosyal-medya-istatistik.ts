import { Component, OnInit } from '@angular/core';
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
}

@Component({
  selector: 'app-sosyal-medya-istatistik',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sosyal-medya-istatistik.html',
  styleUrl:    './sosyal-medya-istatistik.css',
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

  // Sayfalama
  readonly sayfaBoyutu = 20;
  mevcutSayfa = 1;

  readonly hassasiyetLabel = HASSASIYET_LABEL;

  constructor(private http: HttpClient) {}

  ngOnInit(): void { this.yukle(); }

  yukle(): void {
    this.yukleniyor = true;
    this.hata = null;
    this.http.get<SosyalMedyaItem[]>(`${API}/sosyalmedyaolay`).subscribe({
      next: res => {
        this.tumKayitlar = res;
        this.filtrele();
        this.yukleniyor = false;
      },
      error: () => {
        this.hata = 'Veriler yüklenirken hata oluştu.';
        this.yukleniyor = false;
      }
    });
  }

  filtrele(): void {
    this.mevcutSayfa = 1;
    let sonuc = [...this.tumKayitlar];

    if (this.filtreHassasiyet !== '')
      sonuc = sonuc.filter(s => s.hassasiyet === +this.filtreHassasiyet);
    if (this.filtrePlatform.trim())
      sonuc = sonuc.filter(s => s.platform?.toLowerCase().includes(this.filtrePlatform.trim().toLowerCase()));
    if (this.filtreKonu.trim())
      sonuc = sonuc.filter(s => s.konu?.toLowerCase().includes(this.filtreKonu.trim().toLowerCase()));
    if (this.filtreIlgiliKisi.trim())
      sonuc = sonuc.filter(s => s.ilgiliKisiKurum?.toLowerCase().includes(this.filtreIlgiliKisi.trim().toLowerCase()));
    if (this.filtreIl.trim())
      sonuc = sonuc.filter(s => s.il?.toLowerCase().includes(this.filtreIl.trim().toLowerCase()));
    if (this.filtreIlce.trim())
      sonuc = sonuc.filter(s => s.ilce?.toLowerCase().includes(this.filtreIlce.trim().toLowerCase()));
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
