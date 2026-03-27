import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface SandikOlayRecord {
  id: string;
  musahitAdi: string;
  il: string;
  ilce: string;
  mahalle: string;
  sandikNo: number;
  olayKategorisi: string;
  olaySaati: string;
  aciklama: string;
  kanitDosyasi: string | null;
  tarih: string;
  createdAt: string;
}

@Component({
  selector: 'app-secim',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './secim.html',
  styleUrls: ['./secim.css']
})
export class Secim implements OnInit {
  private apiBase = 'http://localhost:5117/api';

  // Form alanlar (ASCII guvenli property isimleri)
  musahitAdi = '';
  il = '';
  ilce = '';
  mahalle = '';
  sandikNo: number | null = null;
  olayKategorisi = '';
  olaySaati = '';
  aciklama = '';
  kanitDosyasiBase64: string | null = null;
  kanitDosyasiAd = '';
  tarih = new Date().toISOString().substring(0, 10);

  // UI durumu
  kayitlar: SandikOlayRecord[] = [];
  gonderiliyor = false;
  hata = '';
  basari = '';
  yukleniyor = true;
  secilenKayit: SandikOlayRecord | null = null;
  modalAcik = false;

  kategoriler = [
    'Musahit Engellenmesi',
    'Mukerrer Oy Denemesi',
    'Kavga / Ariza',
    'Sayim Hatasi',
    'Diger'
  ];

  iller = [
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

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.kayitlariYukle();
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  kayitlariYukle(): void {
    this.yukleniyor = true;
    this.http.get<SandikOlayRecord[]>(`${this.apiBase}/secim/sandik-olay`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => { this.kayitlar = data; this.yukleniyor = false; },
        error: () => { this.hata = 'Kayitlar yuklenemedi.'; this.yukleniyor = false; }
      });
  }

  kategoriSec(k: string): void {
    this.olayKategorisi = this.olayKategorisi === k ? '' : k;
  }

  dosyaSec(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    if (file.size > 5 * 1024 * 1024) {
      this.hata = 'Dosya boyutu 5 MB sinirina asmamalidir.';
      return;
    }
    this.kanitDosyasiAd = file.name;
    const reader = new FileReader();
    reader.onload = (e) => {
      this.kanitDosyasiBase64 = (e.target?.result as string).split(',')[1];
    };
    reader.readAsDataURL(file);
  }

  dosyaTemizle(): void {
    this.kanitDosyasiBase64 = null;
    this.kanitDosyasiAd = '';
  }

  formGecerliMi(): boolean {
    return this.musahitAdi.trim().length >= 3 &&
      !!this.il && !!this.ilce.trim() && !!this.olayKategorisi &&
      !!this.olaySaati && this.aciklama.trim().length > 0 &&
      (this.sandikNo !== null && this.sandikNo > 0);
  }

  kaydet(): void {
    if (!this.formGecerliMi()) return;
    this.gonderiliyor = true;
    this.hata = '';
    this.basari = '';

    const payload = {
      musahitAdi: this.musahitAdi.trim(),
      il: this.il,
      ilce: this.ilce.trim(),
      mahalle: this.mahalle.trim(),
      sandikNo: this.sandikNo,
      olayKategorisi: this.olayKategorisi,
      olaySaati: this.olaySaati + ':00',
      aciklama: this.aciklama.trim(),
      kanitDosyasi: this.kanitDosyasiBase64,
      tarih: new Date(this.tarih).toISOString()
    };

    this.http.post<SandikOlayRecord>(`${this.apiBase}/secim/sandik-olay`, payload, { headers: this.getHeaders() })
      .subscribe({
        next: (yeni) => {
          this.kayitlar.unshift(yeni);
          this.formuSifirla();
          this.basari = 'Sandik olayi basariyla kaydedildi.';
          this.gonderiliyor = false;
        },
        error: (err) => {
          this.hata = err.error?.message || 'Kayit sirasinda hata olustu.';
          this.gonderiliyor = false;
        }
      });
  }

  kayitDetay(kayit: SandikOlayRecord): void {
    this.secilenKayit = kayit;
    this.modalAcik = true;
  }

  modalKapat(): void {
    this.secilenKayit = null;
    this.modalAcik = false;
  }

  kayitSil(id: string): void {
    if (!confirm('Bu kaydi silmek istediginizden emin misiniz?')) return;
    this.http.delete(`${this.apiBase}/secim/sandik-olay/${id}`, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          this.kayitlar = this.kayitlar.filter(k => k.id !== id);
          if (this.secilenKayit?.id === id) this.modalKapat();
        },
        error: () => { this.hata = 'Silme islemi basarisiz oldu.'; }
      });
  }

  private formuSifirla(): void {
    this.musahitAdi = '';
    this.il = '';
    this.ilce = '';
    this.mahalle = '';
    this.sandikNo = null;
    this.olayKategorisi = '';
    this.olaySaati = '';
    this.aciklama = '';
    this.kanitDosyasiBase64 = null;
    this.kanitDosyasiAd = '';
    this.tarih = new Date().toISOString().substring(0, 10);
  }

  tarihFormat(dateStr: string): string {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleDateString('tr-TR');
  }

  kategoriSayisi(k: string): number {
    return this.kayitlar.filter(r => r.olayKategorisi === k).length;
  }

  kategoriRenk(k: string): string {
    const renkler: Record<string, string> = {
      'Musahit Engellenmesi': '#e74c3c',
      'Mukerrer Oy Denemesi': '#e67e22',
      'Kavga / Ariza': '#c0392b',
      'Sayim Hatasi': '#8e44ad',
      'Diger': '#7f8c8d'
    };
    return renkler[k] || '#7f8c8d';
  }
}