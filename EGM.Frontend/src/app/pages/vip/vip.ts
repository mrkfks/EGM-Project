import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface VIPZiyaretRecord {
  id: string;
  ziyaretEdenAdSoyad: string;
  unvan: string;
  baslangicTarihi: string;
  bitisTarihi: string;
  il: string;
  mekan: string;
  hassasiyet: number;
  guvenlikSeviyesi: string;
  gozlemNoktalari: string;
  ziyaretDurumu: number;
}

@Component({
  selector: 'app-vip',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vip.html',
  styleUrls: ['./vip.css']
})
export class VIP implements OnInit {
  private apiBase = 'http://localhost:5117/api';

  // Form alanlari
  ziyaretEdenAdSoyad = '';
  unvan = '';
  mekan = '';
  ziyaretDurumu = 0;
  guvenlikSeviyesi = 'Normal';
  gozlemNoktalari = '';
  baslangicTarihi = new Date().toISOString().substring(0, 16);
  bitisTarihi = new Date().toISOString().substring(0, 16);
  il = '';

  // UI durumu
  kayitlar: VIPZiyaretRecord[] = [];
  gonderiliyor = false;
  hata = '';
  basari = '';
  yukleniyor = true;
  secilenKayit: VIPZiyaretRecord | null = null;
  modalAcik = false;
  aktifSekme: 'form' | 'liste' = 'form';

  readonly unvanlar = [
    'Bakan',
    'Milletvekili',
    'Vali / Kaymakam',
    'Emniyet Muduru',
    'Parti Genel Merkezi Yetkilisi'
  ];

  readonly durumlar = [
    { value: 0, label: 'Planlandi',     renk: '#3498db', bg: '#ebf5fb' },
    { value: 1, label: 'Varis Yapildi', renk: '#27ae60', bg: '#e8f6ed' },
    { value: 2, label: 'Ayrildi',       renk: '#7f8c8d', bg: '#f0f3f4' },
    { value: 3, label: 'Iptal Edildi',  renk: '#e74c3c', bg: '#fdecea' }
  ];

  readonly guvenlikSeviyeleri = [
    { value: 'Normal',           ikon: '🟢', renk: '#27ae60' },
    { value: 'Yogun Guvenlik',  ikon: '🟡', renk: '#f39c12' },
    { value: 'Kritik Durum',    ikon: '🔴', renk: '#e74c3c' }
  ];

  readonly iller = [
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
    this.http.get<VIPZiyaretRecord[]>(`${this.apiBase}/vipziyaret`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => { this.kayitlar = data; this.yukleniyor = false; },
        error: () => { this.hata = 'Kayitlar yuklenemedi.'; this.yukleniyor = false; }
      });
  }

  formGecerliMi(): boolean {
    return this.ziyaretEdenAdSoyad.trim().length >= 2 &&
      !!this.unvan && !!this.mekan.trim() && !!this.il;
  }

  kaydet(): void {
    if (!this.formGecerliMi()) return;
    this.gonderiliyor = true;
    this.hata = '';
    this.basari = '';

    const baslangic = new Date(this.baslangicTarihi);
    const bitis = new Date(this.bitisTarihi);
    if (bitis <= baslangic) {
      this.hata = 'Bitis tarihi baslangic tarihinden sonra olmalidir.';
      this.gonderiliyor = false;
      return;
    }

    const payload = {
      ziyaretEdenAdSoyad: this.ziyaretEdenAdSoyad.trim(),
      unvan: this.unvan,
      baslangicTarihi: baslangic.toISOString(),
      bitisTarihi: bitis.toISOString(),
      il: this.il,
      mekan: this.mekan.trim(),
      hassasiyet: 0,
      guvenlikSeviyesi: this.guvenlikSeviyesi,
      gozlemNoktalari: this.gozlemNoktalari.trim(),
      ziyaretDurumu: this.ziyaretDurumu
    };

    this.http.post<VIPZiyaretRecord>(`${this.apiBase}/vipziyaret`, payload, { headers: this.getHeaders() })
      .subscribe({
        next: (yeni) => {
          this.kayitlar.unshift(yeni);
          this.formuSifirla();
          this.basari = 'Ziyaret kaydi basariyla olusturuldu.';
          this.gonderiliyor = false;
          this.aktifSekme = 'liste';
        },
        error: (err) => {
          this.hata = err.error?.message || 'Kayit sirasinda hata olustu.';
          this.gonderiliyor = false;
        }
      });
  }

  kayitDetay(kayit: VIPZiyaretRecord): void {
    this.secilenKayit = kayit;
    this.modalAcik = true;
  }

  modalKapat(): void {
    this.secilenKayit = null;
    this.modalAcik = false;
  }

  kayitSil(id: string): void {
    if (!confirm('Bu kaydi silmek istediginizden emin misiniz?')) return;
    this.http.delete(`${this.apiBase}/vipziyaret/${id}`, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          this.kayitlar = this.kayitlar.filter(k => k.id !== id);
          if (this.secilenKayit?.id === id) this.modalKapat();
        },
        error: () => { this.hata = 'Silme islemi basarisiz oldu.'; }
      });
  }

  private formuSifirla(): void {
    this.ziyaretEdenAdSoyad = '';
    this.unvan = '';
    this.mekan = '';
    this.il = '';
    this.ziyaretDurumu = 0;
    this.guvenlikSeviyesi = 'Normal';
    this.gozlemNoktalari = '';
    this.baslangicTarihi = new Date().toISOString().substring(0, 16);
    this.bitisTarihi = new Date().toISOString().substring(0, 16);
  }

  getDurumBilgi(val: number) {
    return this.durumlar.find(d => d.value === val) ?? this.durumlar[0];
  }

  tarihFormat(dateStr: string): string {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
  }

  durumSayisi(val: number): number {
    return this.kayitlar.filter(k => k.ziyaretDurumu === val).length;
  }
}