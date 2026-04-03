import { Component, OnInit } from '@angular/core';
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

interface OlayItem {
  id: string;
  baslik?: string;
  olayTuru?: string;
  organizatorAd?: string;
  konuAd?: string;
  il?: string;
  ilce?: string;
  mekan?: string;
  tarih: string;
  baslangicSaati?: string;
  bitisSaati?: string;
  hassasiyet: number;
  durum: number;
  katilimciSayisi?: number;
  gozaltiSayisi?: number;
  sehitOluSayisi?: number;
  riskPuani?: number;
  kaynakKurum?: string;
  evrakNumarasi?: string;
  aciklama?: string;
}

@Component({
  selector: 'app-sokak-istatistik',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sokak-istatistik.html',
  styleUrl:    './sokak-istatistik.css',
})
export class SokakIstatistik implements OnInit {

  tumKayitlar: OlayItem[] = [];
  filtreli:    OlayItem[] = [];
  yukleniyor = false;
  hata: string | null = null;

  // Filtreler
  filtreBaslik       = '';
  filtreOlayTuru     = '';
  filtreOrganizator  = '';
  filtreKonu         = '';
  filtreIl           = '';
  filtreIlce         = '';
  filtreMekan        = '';
  filtreHassasiyet   = '';
  filtreDurum        = '';
  filtreTarih1       = '';
  filtreTarih2       = '';
  filtreKaynakKurum  = '';
  filtreEvrakNo      = '';

  // Sayfalama
  readonly sayfaBoyutu = 20;
  mevcutSayfa = 1;

  readonly hassasiyetLabel = HASSASIYET_LABEL;
  readonly durumLabel      = DURUM_LABEL;

  constructor(private http: HttpClient) {}

  ngOnInit(): void { this.yukle(); }

  yukle(): void {
    this.yukleniyor = true;
    this.hata = null;
    // Tüm kayıtları al (max 500), filtreleme client-side
    this.http.get<any>(`${API}/olay?sayfa=1&sayfaBoyutu=500`).subscribe({
      next: res => {
        this.tumKayitlar = res.items ?? [];
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
    let s = [...this.tumKayitlar];

    if (this.filtreBaslik.trim())
      s = s.filter(o => o.baslik?.toLowerCase().includes(this.filtreBaslik.trim().toLowerCase()));
    if (this.filtreOlayTuru.trim())
      s = s.filter(o => o.olayTuru?.toLowerCase().includes(this.filtreOlayTuru.trim().toLowerCase()));
    if (this.filtreOrganizator.trim())
      s = s.filter(o => o.organizatorAd?.toLowerCase().includes(this.filtreOrganizator.trim().toLowerCase()));
    if (this.filtreKonu.trim())
      s = s.filter(o => o.konuAd?.toLowerCase().includes(this.filtreKonu.trim().toLowerCase()));
    if (this.filtreIl.trim())
      s = s.filter(o => o.il?.toLowerCase().includes(this.filtreIl.trim().toLowerCase()));
    if (this.filtreIlce.trim())
      s = s.filter(o => o.ilce?.toLowerCase().includes(this.filtreIlce.trim().toLowerCase()));
    if (this.filtreMekan.trim())
      s = s.filter(o => o.mekan?.toLowerCase().includes(this.filtreMekan.trim().toLowerCase()));
    if (this.filtreHassasiyet !== '')
      s = s.filter(o => o.hassasiyet === +this.filtreHassasiyet);
    if (this.filtreDurum !== '')
      s = s.filter(o => o.durum === +this.filtreDurum);
    if (this.filtreTarih1)
      s = s.filter(o => o.tarih && new Date(o.tarih) >= new Date(this.filtreTarih1));
    if (this.filtreTarih2) {
      const t2 = new Date(this.filtreTarih2); t2.setHours(23, 59, 59);
      s = s.filter(o => o.tarih && new Date(o.tarih) <= t2);
    }
    if (this.filtreKaynakKurum.trim())
      s = s.filter(o => o.kaynakKurum?.toLowerCase().includes(this.filtreKaynakKurum.trim().toLowerCase()));
    if (this.filtreEvrakNo.trim())
      s = s.filter(o => o.evrakNumarasi?.toLowerCase().includes(this.filtreEvrakNo.trim().toLowerCase()));

    this.filtreli = s;
  }

  filtreleriSifirla(): void {
    this.filtreBaslik = this.filtreOlayTuru = this.filtreOrganizator = this.filtreKonu = '';
    this.filtreIl = this.filtreIlce = this.filtreMekan = this.filtreHassasiyet = '';
    this.filtreDurum = this.filtreTarih1 = this.filtreTarih2 = '';
    this.filtreKaynakKurum = this.filtreEvrakNo = '';
    this.filtrele();
  }

  get toplamSayfa(): number {
    return Math.max(1, Math.ceil(this.filtreli.length / this.sayfaBoyutu));
  }

  get sayfaKayitlari(): OlayItem[] {
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

  durumClass(d: number): string {
    return ['durum-planlandi', 'durum-gerceklesti', 'durum-iptal'][d] ?? '';
  }
}


