import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl + '/api';

const HASSASIYET_RENK: Record<number, string> = {
  0: '#27ae60',
  1: '#f39c12',
  2: '#e74c3c',
  3: '#8e44ad',
};

const DURUM_ETIKET: Record<number, string> = {
  0: 'Planlandı',
  1: 'Gerçekleşti',
  2: 'İptal',
};

interface OnemliGun {
  id: string;
  tarih: string; // YYYY-MM-DD
  baslik: string;
  renk: string;
}

interface OlayItem {
  id: string;
  olayTuru?: string;
  tarih: string;
  baslangicSaati?: string;
  bitisSaati?: string;
  il?: string;
  ilce?: string;
  mekan?: string;
  hassasiyet: number;
  durum: number;
}

interface GunHucresi {
  gun: number | null;
  tarih: Date | null;
  olaylar: OlayItem[];
  bugun: boolean;
}

const AY_ADLARI = [
  'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
  'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık',
];

@Component({
  selector: 'app-ajanda',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ajanda.html',
  styleUrls: ['./ajanda.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Ajanda implements OnInit {
  yil = new Date().getFullYear();
  ay = new Date().getMonth();
  ayAdi = AY_ADLARI[this.ay];
  takvimHucreleri: GunHucresi[] = [];
  secilenGun: GunHucresi | null = null;
  yukleniyor = false;

  // Önemli günler
  private readonly STORAGE_KEY = 'egm_onemli_gunler';
  onemliGunMap: Record<string, OnemliGun[]> = {};
  ekleFormAcik = false;
  yeniBaslik = '';
  yeniRenk = '#e74c3c';
  readonly renkSecenekleri = ['#e74c3c','#e67e22','#f1c40f','#27ae60','#3498db','#8e44ad','#1a3a5c'];

  readonly gunAdlari = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz'];
  readonly hassasiyetRenk = HASSASIYET_RENK;
  readonly durumEtiket = DURUM_ETIKET;

  private tumOlaylar: OlayItem[] = [];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef, private router: Router) {}

  ngOnInit(): void {
    this.loadOnemliGunler();
    this.olaylariYukle();
  }

  olaylariYukle(): void {
    this.yukleniyor = true;
    this.http.get<OlayItem[]>(`${API}/olay`).subscribe({
      next: (data) => {
        this.tumOlaylar = Array.isArray(data) ? data : [];
        this.takvimOlustur();
        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
    });
  }

  oncekiAy(): void {
    if (this.ay === 0) { this.ay = 11; this.yil--; }
    else { this.ay--; }
    this.ayAdi = AY_ADLARI[this.ay];
    this.secilenGun = null;
    this.takvimOlustur();
    this.cdr.markForCheck();
  }

  sonrakiAy(): void {
    if (this.ay === 11) { this.ay = 0; this.yil++; }
    else { this.ay++; }
    this.ayAdi = AY_ADLARI[this.ay];
    this.secilenGun = null;
    this.takvimOlustur();
    this.cdr.markForCheck();
  }

  gunSec(h: GunHucresi): void {
    if (!h.gun) return;
    const sameMi = this.secilenGun?.tarih?.getTime() === h.tarih?.getTime();
    this.secilenGun = sameMi ? null : h;
    this.ekleFormAcik = false;
    this.yeniBaslik = '';
    this.cdr.markForCheck();
  }

  getOnemliGunler(tarih: Date | null): OnemliGun[] {
    if (!tarih) return [];
    return this.onemliGunMap[tarih.toISOString().slice(0, 10)] ?? [];
  }

  onemliGunEkle(): void {
    if (!this.secilenGun?.tarih || !this.yeniBaslik.trim()) return;
    const tarihStr = this.secilenGun.tarih.toISOString().slice(0, 10);
    const yeni: OnemliGun = {
      id: Date.now().toString(),
      tarih: tarihStr,
      baslik: this.yeniBaslik.trim(),
      renk: this.yeniRenk,
    };
    this.onemliGunMap = {
      ...this.onemliGunMap,
      [tarihStr]: [...(this.onemliGunMap[tarihStr] ?? []), yeni],
    };
    this.saveOnemliGunler();
    this.yeniBaslik = '';
    this.ekleFormAcik = false;
    this.cdr.markForCheck();
  }

  onemliGunSil(gun: OnemliGun, event: Event): void {
    event.stopPropagation();
    const arr = (this.onemliGunMap[gun.tarih] ?? []).filter(g => g.id !== gun.id);
    this.onemliGunMap = { ...this.onemliGunMap, [gun.tarih]: arr };
    this.saveOnemliGunler();
    this.cdr.markForCheck();
  }

  private loadOnemliGunler(): void {
    try {
      const raw = localStorage.getItem(this.STORAGE_KEY);
      this.onemliGunMap = raw ? JSON.parse(raw) : {};
    } catch { this.onemliGunMap = {}; }
  }

  private saveOnemliGunler(): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.onemliGunMap));
  }

  olayDetay(id: string): void {
    this.router.navigate(['/olay'], { queryParams: { id } });
  }

  private takvimOlustur(): void {
    const ilkGun = new Date(this.yil, this.ay, 1);
    const sonGun = new Date(this.yil, this.ay + 1, 0);
    const bugun = new Date();

    // Pazartesi=0 başlangıcı için ofset (JS: 0=Pazar)
    let baslangicOffset = ilkGun.getDay() - 1;
    if (baslangicOffset < 0) baslangicOffset = 6;

    const hucreler: GunHucresi[] = [];

    // Boş hücreler (ayın ilk haftasından önceki günler)
    for (let i = 0; i < baslangicOffset; i++) {
      hucreler.push({ gun: null, tarih: null, olaylar: [], bugun: false });
    }

    for (let g = 1; g <= sonGun.getDate(); g++) {
      const tarih = new Date(this.yil, this.ay, g);
      const tarihStr = tarih.toISOString().slice(0, 10);
      const olaylar = this.tumOlaylar.filter(o => o.tarih?.slice(0, 10) === tarihStr);
      const bugunMu = tarih.toDateString() === bugun.toDateString();
      hucreler.push({ gun: g, tarih, olaylar, bugun: bugunMu });
    }

    // Son satırı 7'ye tamamla
    while (hucreler.length % 7 !== 0) {
      hucreler.push({ gun: null, tarih: null, olaylar: [], bugun: false });
    }

    this.takvimHucreleri = hucreler;
  }
}
