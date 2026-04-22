import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { environment } from '../../../environments/environment';

interface Olay {
  id: string;
  olayNo: string;
  durum: number;
  baslangicTarihi: string;
  bitisTarihi?: string;
  turAd?: string;
  sekilAd?: string;
  konuAd?: string;
  organizatorAd?: string;
  aciklama?: string;
  locations: { il?: string; ilce?: string; mahalle?: string; mekan?: string }[];
}

@Component({
  selector: 'app-yaklasan-olaylar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './yaklasan-olaylar.html',
  styleUrls: ['./yaklasan-olaylar.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class YaklasanOlaylar implements OnInit {
  planlananlar: Olay[] = [];
  gecmisOlaylar: Olay[] = [];
  yukleniyor = true;
  hata: string | null = null;

  private readonly API = environment.apiUrl + '/api';

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.yukle();
  }

  yukle(): void {
    this.yukleniyor = true;
    this.hata = null;

    const simdi = new Date();
    const sonra24 = new Date(simdi.getTime() + 24 * 60 * 60 * 1000);
    const once24 = new Date(simdi.getTime() - 24 * 60 * 60 * 1000);

    forkJoin({
      planlanan:    this.http.get<Olay[]>(`${this.API}/olay?durum=0&sayfaBoyutu=500`),
      gerceklesen:  this.http.get<Olay[]>(`${this.API}/olay?durum=2&sayfaBoyutu=500`),
      iptal:        this.http.get<Olay[]>(`${this.API}/olay?durum=3&sayfaBoyutu=500`),
    }).subscribe({
      next: ({ planlanan, gerceklesen, iptal }) => {
        // Planlanan: başlangıç tarihi şimdi ile +24 saat arasındaki VEYA başlangıç tarihi şimdiden sonraki tüm planlanmış olaylar
        this.planlananlar = planlanan
          .filter(o => {
            const t = new Date(o.baslangicTarihi);
            return t >= once24 && t <= sonra24;
          })
          .sort((a, b) => new Date(a.baslangicTarihi).getTime() - new Date(b.baslangicTarihi).getTime());

        this.gecmisOlaylar = [...gerceklesen, ...iptal]
          .filter(o => {
            const t = new Date(o.baslangicTarihi);
            return t >= once24 && t <= sonra24;
          })
          .sort((a, b) => new Date(b.baslangicTarihi).getTime() - new Date(a.baslangicTarihi).getTime());

        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.hata = 'Olaylar yüklenirken bir hata oluştu.';
        this.yukleniyor = false;
        this.cdr.markForCheck();
      }
    });
  }

  konumStr(o: Olay): string {
    const l = o.locations?.[0];
    if (!l) return '—';
    return [l.il, l.ilce, l.mahalle, l.mekan].filter(Boolean).join(' / ');
  }

  formatTarih(dt: string): string {
    return new Date(dt).toLocaleString('tr-TR', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }

  kalanSure(dt: string): string {
    const diff = new Date(dt).getTime() - Date.now();
    if (diff <= 0) return 'Başladı';
    const h = Math.floor(diff / 3600000);
    const m = Math.floor((diff % 3600000) / 60000);
    return h > 0 ? `${h} sa ${m} dk` : `${m} dk`;
  }

  durumEtiket(durum: number): string {
    return durum === 2 ? 'Gerçekleşen' : 'İptal Edildi';
  }

  durumSinif(durum: number): string {
    return durum === 2 ? 'badge-gerceklesen' : 'badge-iptal';
  }
}
