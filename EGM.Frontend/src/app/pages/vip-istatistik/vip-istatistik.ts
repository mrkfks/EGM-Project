import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl + '/api';

const HASSASIYET_LABEL: Record<number, string> = {
  0: 'Düşük', 1: 'Orta', 2: 'Yüksek', 3: 'Kritik'
};
const DURUM_LABEL: Record<number, string> = {
  0: 'Planlandı', 1: 'Gerçekleşti', 2: 'İptal'
};
const GUVENLIK_LABEL: Record<number, string | undefined> = {
  0: 'Normal', 1: 'Artırılmış', 2: 'Yüksek', 3: 'Maksimum'
};

interface VIPZiyaretItem {
  id: string;
  ziyaretEdenAdSoyad?: string;
  unvan?: string;
  baslangicTarihi?: string;
  bitisTarihi?: string;
  il?: string;
  mekan?: string;
  hassasiyet: number;
  guvenlikSeviyesi: number;
  gozlemNoktalari?: string;
  ziyaretDurumu: number;
  takipNo?: string;
}

@Component({
  selector: 'app-vip-istatistik',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vip-istatistik.html',
  styleUrl:    './vip-istatistik.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VipIstatistik implements OnInit {

  tumKayitlar: VIPZiyaretItem[] = [];
  filtreli:    VIPZiyaretItem[] = [];
  yukleniyor = false;
  hata: string | null = null;

  // Filtreler
  filtreDurum      = '';
  filtreHassasiyet = '';
  filtreIl         = '';
  filtreTarih1     = '';
  filtreTarih2     = '';

  // Sayfalama
  readonly sayfaBoyutu = 20;
  mevcutSayfa = 1;

  readonly hassasiyetLabel = HASSASIYET_LABEL;
  readonly durumLabel      = DURUM_LABEL;
  readonly guvenlikLabel   = GUVENLIK_LABEL;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void { this.yukle(); }

  yukle(): void {
    this.yukleniyor = true;
    this.hata = null;
    this.http.get<VIPZiyaretItem[]>(`${API}/vipziyaret`).subscribe({
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

    if (this.filtreDurum !== '') {
      sonuc = sonuc.filter(v => v.ziyaretDurumu === +this.filtreDurum);
    }
    if (this.filtreHassasiyet !== '') {
      sonuc = sonuc.filter(v => v.hassasiyet === +this.filtreHassasiyet);
    }
    if (this.filtreIl !== '') {
      sonuc = sonuc.filter(v => v.il === this.filtreIl);
    }
    if (this.filtreTarih1) {
      const t1 = new Date(this.filtreTarih1);
      sonuc = sonuc.filter(v => v.baslangicTarihi && new Date(v.baslangicTarihi) >= t1);
    }
    if (this.filtreTarih2) {
      const t2 = new Date(this.filtreTarih2);
      t2.setHours(23, 59, 59);
      sonuc = sonuc.filter(v => v.baslangicTarihi && new Date(v.baslangicTarihi) <= t2);
    }
    this.filtreli = sonuc;
  }

  get toplamSayfa(): number {
    return Math.max(1, Math.ceil(this.filtreli.length / this.sayfaBoyutu));
  }

  get sayfaKayitlari(): VIPZiyaretItem[] {
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

  get ilSecenekler(): string[] {
    return [...new Set(this.tumKayitlar.map(k => k.il ?? '').filter(v => v))].sort();
  }

  hassasiyetClass(h: number): string {
    return ['hass-dusuk', 'hass-orta', 'hass-yuksek', 'hass-kritik'][h] ?? '';
  }

  durumClass(d: number): string {
    return ['durum-planlandi', 'durum-gerceklesti', 'durum-iptal'][d] ?? '';
  }
}
