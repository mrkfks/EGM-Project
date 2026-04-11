import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { KonuService, Konu } from '../../services/konu.service';

const API = 'http://localhost:5117/api';

interface SosyalMedyaForm {
  kullaniciAdi: string;
  paylasimLinki: string;
  platform: string;
  konu: string;
  il: string;
  ilce: string;
  mahalle: string;
  konum: string;
  paylasimTarihi: string;
  icerikOzeti: string;
  hassasiyet: number;
  ekranGoruntusuBase64: string;
  ekranGoruntusuAd: string;
}

@Component({
  selector: 'app-socialmedia',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './socialmedia.html',
  styleUrls: ['./socialmedia.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Socialmedia implements OnInit {
  private apiBase = API;

  kaydediliyor = false;
  basariMesaji = '';
  hataMesaji = '';
  takipNo: string | null = null;

  konular: Konu[] = [];
  konularYukleniyor = false;

  form: SosyalMedyaForm = {
    kullaniciAdi: '',
    paylasimLinki: '',
    platform: '',
    konu: '',
    il: '',
    ilce: '',
    mahalle: '',
    konum: '',
    paylasimTarihi: new Date().toISOString().slice(0, 16),
    icerikOzeti: '',
    hassasiyet: 0,
    ekranGoruntusuBase64: '',
    ekranGoruntusuAd: '',
  };

  tumIller: { name: string; osmId: number }[] = [];
  filtreliIlceler: { name: string; osmId: number }[] = [];
  filtreliMahalleler: { name: string; osmId: number }[] = [];

  platformlar: string[] = [
    'Twitter / X',
    'Facebook',
    'Instagram',
    'YouTube',
    'TikTok',
    'Telegram',
    'WhatsApp',
    'LinkedIn',
    'Diger',
  ];

  iller: string[] = [
    'Adana','Adiyaman','Afyonkarahisar','Agri','Amasya','Ankara','Antalya',
    'Artvin','Aydin','Balikesir','Bilecik','Bingol','Bitlis','Bolu','Burdur',
    'Bursa','Canakkale','Cankiri','Corum','Denizli','Diyarbakir','Edirne',
    'Elazig','Erzincan','Erzurum','Eskisehir','Gaziantep','Giresun',
    'Gumushane','Hakkari','Hatay','Isparta','Mersin','Istanbul','Izmir',
    'Kars','Kastamonu','Kayseri','Kirklareli','Kirsehir','Kocaeli','Konya',
    'Kutahya','Malatya','Manisa','Kahramanmaras','Mardin','Mugla','Mus',
    'Nevsehir','Nigde','Ordu','Rize','Sakarya','Samsun','Siirt','Sinop',
    'Sivas','Tekirdag','Tokat','Trabzon','Tunceli','Sanliurfa','Usak',
    'Van','Yozgat','Zonguldak','Aksaray','Bayburt','Karaman','Kirikkale',
    'Batman','Sirnak','Bartin','Ardahan','Igdir','Yalova','Karabuk',
    'Kilis','Osmaniye','Duzce'
  ];

  yeniPlatformAcik = false;
  yeniPlatform = '';

  platformEkle(): void {
    const ad = this.yeniPlatform.trim();
    if (!ad || this.platformlar.includes(ad)) return;
    this.platformlar.push(ad);
    this.form.platform = ad;
    this.yeniPlatform = '';
    this.yeniPlatformAcik = false;
  }

  platformSil(p: string): void {
    this.platformlar = this.platformlar.filter(x => x !== p);
    if (this.form.platform === p) this.form.platform = '';
  }

  constructor(private http: HttpClient, private konuService: KonuService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.konularYukleniyor = true;
    this.konuService.getAll().subscribe({
      next: res => { this.konular = res; this.konularYukleniyor = false; this.cdr.markForCheck(); },
      error: () => { this.konularYukleniyor = false; this.cdr.markForCheck(); }
    });
    this.http.get<any[]>(`${API}/geo/provinces-geopackage`).subscribe({
      next: res => { this.tumIller = res; this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  ilDegisti(): void {
    this.filtreliIlceler = [];
    this.filtreliMahalleler = [];
    this.form.ilce = '';
    this.form.mahalle = '';
    this.form.konum = '';
    if (!this.form.il) { this.cdr.markForCheck(); return; }
    this.http.get<any[]>(`${API}/geo/districts-geopackage?province=${encodeURIComponent(this.form.il)}`)
      .pipe(catchError(() => of([])))
      .subscribe(districts => { this.filtreliIlceler = districts; this.cdr.markForCheck(); });
  }

  ilceDegisti(): void {
    this.filtreliMahalleler = [];
    this.form.mahalle = '';
    this.form.konum = '';
    if (!this.form.ilce) { this.cdr.markForCheck(); return; }
    this.http.get<any[]>(`${API}/geo/neighborhoods-geopackage?district=${encodeURIComponent(this.form.ilce)}`)
      .pipe(catchError(() => of([])))
      .subscribe(neighborhoods => { this.filtreliMahalleler = neighborhoods; this.cdr.markForCheck(); });
  }

  mahalleDegisti(): void {
    this.form.konum = '';
    if (!this.form.il || !this.form.ilce || !this.form.mahalle) { this.cdr.markForCheck(); return; }
    this.http.get<any>(`${API}/geo/get-coordinates?provinceName=${encodeURIComponent(this.form.il)}&districtName=${encodeURIComponent(this.form.ilce)}&neighborhoodName=${encodeURIComponent(this.form.mahalle)}`)
      .pipe(catchError(() => of(null)))
      .subscribe(coords => {
        if (coords?.latitude && coords?.longitude) {
          this.form.konum = `${coords.latitude.toFixed(6)}, ${coords.longitude.toFixed(6)}`;
        } else {
          this.form.konum = `${this.form.mahalle}, ${this.form.ilce}, ${this.form.il}`;
        }
        this.cdr.markForCheck();
      });
  }

  gorselSec(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    if (!file.type.startsWith('image/')) {
      this.hataMesaji = 'Lutfen gecerli bir gorsel dosyasi secin.';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.hataMesaji = 'Gorsel boyutu 5 MB den buyuk olamaz.';
      return;
    }
    this.hataMesaji = '';
    this.form.ekranGoruntusuAd = file.name;
    const reader = new FileReader();
    reader.onload = (e) => {
      this.form.ekranGoruntusuBase64 = e.target?.result as string;
      this.cdr.markForCheck();
    };
    reader.readAsDataURL(file);
  }

  gorselKaldir(): void {
    this.form.ekranGoruntusuBase64 = '';
    this.form.ekranGoruntusuAd = '';
  }

  kaydet(): void {
    if (!this.form.kullaniciAdi.trim()) {
      this.hataMesaji = 'Kullanici adi zorunludur.';
      return;
    }
    if (!this.form.paylasimLinki.trim()) {
      this.hataMesaji = 'Paylasim linki zorunludur.';
      return;
    }
    if (!this.form.platform) {
      this.hataMesaji = 'Platform secimi zorunludur.';
      return;
    }
    if (!this.form.icerikOzeti.trim()) {
      this.hataMesaji = 'Paylasim aciklamasi zorunludur.';
      return;
    }

    this.kaydediliyor = true;
    this.hataMesaji = '';
    this.basariMesaji = '';

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    const payload = {
      olayId: null,
      platform: this.form.platform,
      konu: this.form.konu || null,
      il: this.form.il || null,
      ilce: this.form.ilce || null,
      paylasimLinki: this.form.paylasimLinki,
      paylasimTarihi: this.form.paylasimTarihi.length === 16
        ? this.form.paylasimTarihi + ':00'
        : this.form.paylasimTarihi,
      icerikOzeti: this.form.icerikOzeti,
      ilgiliKisiKurum: this.form.kullaniciAdi,
      ekranGoruntusu: this.form.ekranGoruntusuBase64 || null,
      hassasiyet: this.form.hassasiyet,
    };

    this.http
      .post<any>(`${this.apiBase}/sosyalmedyaolay`, payload, { headers })
      .subscribe({
        next: (res) => {
          this.takipNo = res?.takipNo ?? null;
          this.basariMesaji = 'Sosyal medya olayi basariyla kaydedildi.';
          this.kaydediliyor = false;
          this.formSifirla();
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.hataMesaji = err?.error?.title || 'Kayit sirasinda hata olustu.';
          this.kaydediliyor = false;
          this.cdr.markForCheck();
        },
      });
  }

  private formSifirla(): void {
    this.form = {
      kullaniciAdi: '',
      paylasimLinki: '',
      platform: '',
      konu: '',
      il: '',
      ilce: '',
      mahalle: '',
      konum: '',
      paylasimTarihi: new Date().toISOString().slice(0, 16),
      icerikOzeti: '',
      hassasiyet: 0,
      ekranGoruntusuBase64: '',
      ekranGoruntusuAd: '',
    };
  }
}