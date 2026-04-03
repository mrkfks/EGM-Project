import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl;

interface IcmalVerisi {
  tur: string;
  eylemSayisi: string;
  katilimSayisi: string;
  gozaltiSayisi: string;
  oluSayisi: string;
}

interface GerceklesenDetay {
  sn: number;
  il: string;
  eylemEtkinlik: string;
  saat: string;
  organizeEden: string;
  aciklama: string;
  katilimSayisi: string;
}

interface BeklenenDetay {
  sn: number;
  il: string;
  yer: string;
  eylemEtkinlik: string;
  saat: string;
  organizeEden: string;
  aciklama: string;
}

interface OperasyonelDetay {
  sn: number;
  il: string;
  tarih: string;
  supheliSayisi: string;
  gozaltiSayisi: string;
  aciklama: string;
}

// API response shape (camelCase from .NET)
interface GunlukBultenDto {
  tarih: string;
  sonrakiGunTarih: string;
  icmalVerileri: { tur: string; eylemSayisi: number; katilimSayisi: number; gozaltiSayisi: number; oluSayisi: number }[];
  gerceklesenDetaylar: { sn: number; il: string; eylemEtkinlik: string; saat: string; organizeEden: string; aciklama: string; katilimSayisi: number }[];
  beklenenDetaylar: { sn: number; il: string; yer: string; eylemEtkinlik: string; saat: string; organizeEden: string; aciklama: string }[];
  operasyonelFaaliyetler: { sn: number; il: string; tarih: string; supheliSayisi: number; gozaltiSayisi: number; aciklama: string }[];
}

@Component({
  selector: 'app-raporlar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './raporlar.html',
  styleUrls: ['./raporlar.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Raporlar implements OnInit {
  className = 'LEGAL GUNLUK BULTEN';
  tarih = '';
  sonrakiGunTarih = '';
  seciliTarih = '';

  isLoading = false;
  errorMessage = '';

  icmalVerileri: IcmalVerisi[] = [];
  gerceklesenDetaylar: GerceklesenDetay[] = [];
  beklenenDetaylar: BeklenenDetay[] = [];
  operasyonelFaaliyetler: OperasyonelDetay[] = [];

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.setToday();
  }

  setToday(): void {
    const now = new Date();
    this.seciliTarih = now.toISOString().split('T')[0];
    this.tarih = this.toDisplayDate(now);
    this.sonrakiGunTarih = this.toDisplayDate(new Date(now.getTime() + 86_400_000));
    this.loadBulten(this.seciliTarih);
  }

  onTarihChange(): void {
    if (!this.seciliTarih) return;
    const d = new Date(this.seciliTarih + 'T00:00:00');
    this.tarih = this.toDisplayDate(d);
    this.sonrakiGunTarih = this.toDisplayDate(new Date(d.getTime() + 86_400_000));
    this.loadBulten(this.seciliTarih);
  }

  loadBulten(tarih: string): void {
    this.isLoading = true;
    this.errorMessage = '';
    const token = localStorage.getItem('token') || '';
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.get<GunlukBultenDto>(
      `${API}/api/raporlar/gunluk-bulten?tarih=${tarih}`,
      { headers }
    ).subscribe({
      next: (data) => {
        this.tarih           = data.tarih;
        this.sonrakiGunTarih = data.sonrakiGunTarih;

        this.icmalVerileri = data.icmalVerileri.map(v => ({
          tur:           v.tur,
          eylemSayisi:   v.eylemSayisi.toString(),
          katilimSayisi: v.katilimSayisi.toString(),
          gozaltiSayisi: v.gozaltiSayisi === 0 ? '(-)' : v.gozaltiSayisi.toString(),
          oluSayisi:     v.oluSayisi === 0     ? '(-)' : v.oluSayisi.toString(),
        }));

        this.gerceklesenDetaylar = data.gerceklesenDetaylar.map(d => ({
          sn:            d.sn,
          il:            d.il,
          eylemEtkinlik: d.eylemEtkinlik,
          saat:          d.saat,
          organizeEden:  d.organizeEden,
          aciklama:      d.aciklama,
          katilimSayisi: d.katilimSayisi.toString(),
        }));

        this.beklenenDetaylar = data.beklenenDetaylar.map(b => ({
          sn:            b.sn,
          il:            b.il,
          yer:           b.yer,
          eylemEtkinlik: b.eylemEtkinlik,
          saat:          b.saat,
          organizeEden:  b.organizeEden,
          aciklama:      b.aciklama,
        }));

        this.operasyonelFaaliyetler = data.operasyonelFaaliyetler.map(f => ({
          sn:            f.sn,
          il:            f.il,
          tarih:         f.tarih,
          supheliSayisi: f.supheliSayisi.toString(),
          gozaltiSayisi: f.gozaltiSayisi.toString(),
          aciklama:      f.aciklama,
        }));

        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.errorMessage = 'Bülten verisi yüklenemedi. Bağlantıyı kontrol edin.';
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  printReport(): void {
    if (typeof window !== 'undefined') {
      window.print();
    }
  }

  trackBySn(_index: number, item: { sn: number }): number {
    return item.sn;
  }

  private toDisplayDate(value: Date): string {
    return value.toLocaleDateString('tr-TR');
  }
}

