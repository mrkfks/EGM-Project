import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
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
  takipNo?: string | null;
}

@Component({
  selector: 'app-vip',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vip.html',
  styleUrls: ['./vip.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
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
  bitisTarihi = new Date(Date.now() + 60 * 60 * 1000).toISOString().substring(0, 16);
  il = '';
  hassasiyet = 0;

  // UI durumu
  kayitlar: VIPZiyaretRecord[] = [];
  gonderiliyor = false;
  hata = '';
  basari = '';
  takipNo: string | null = null;
  yukleniyor = true;

  activeForm: 'planned' | 'completed' = 'planned';
  selectedPlanId: string | null = null;

  unvanlar = [
    'Bakan',
    'Milletvekili',
    'Vali / Kaymakam',
    'Emniyet Muduru',
    'Parti Genel Merkezi Yetkilisi'
  ];

  yeniUnvanAcik = false;
  yeniUnvan = '';

  unvanSil(u: string): void {
    this.unvanlar = this.unvanlar.filter(x => x !== u);
    if (this.unvan === u) this.unvan = '';
  }

  unvanEkle(): void {
    const v = this.yeniUnvan.trim();
    if (v && !this.unvanlar.includes(v)) {
      this.unvanlar = [...this.unvanlar, v];
    }
    this.unvan = v || this.unvan;
    this.yeniUnvanAcik = false;
    this.yeniUnvan = '';
  }

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

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  get planlananlar(): VIPZiyaretRecord[] {
    return this.kayitlar.filter(k => k != null && k.ziyaretDurumu === 0);
  }

  get gerceklesen(): VIPZiyaretRecord[] {
    return this.kayitlar.filter(k => k != null && k.ziyaretDurumu !== 0);
  }

  switchToPlannedForm(): void {
    this.activeForm = 'planned';
    this.selectedPlanId = null;
    this.hata = '';
    this.basari = '';
    this.formuSifirla();
    this.ziyaretDurumu = 0;
  }

  switchToCompletedForm(): void {
    this.activeForm = 'completed';
    this.selectedPlanId = null;
    this.hata = '';
    this.basari = '';
    this.formuSifirla();
    this.ziyaretDurumu = 1;
  }

  selectPlanForCompletion(kayit: VIPZiyaretRecord): void {
    this.selectedPlanId = kayit.id;
    this.activeForm = 'completed';
    this.hata = '';
    this.basari = '';
    this.ziyaretEdenAdSoyad = kayit.ziyaretEdenAdSoyad;
    this.unvan = kayit.unvan;
    this.il = kayit.il;
    this.mekan = kayit.mekan;
    this.hassasiyet = kayit.hassasiyet;
    this.guvenlikSeviyesi = kayit.guvenlikSeviyesi;
    this.gozlemNoktalari = kayit.gozlemNoktalari ?? '';
    this.baslangicTarihi = kayit.baslangicTarihi ? kayit.baslangicTarihi.substring(0, 16) : this.baslangicTarihi;
    this.bitisTarihi = kayit.bitisTarihi ? kayit.bitisTarihi.substring(0, 16) : this.bitisTarihi;
    this.ziyaretDurumu = 1;
  }

  iptalEt(): void {
    if (!this.selectedPlanId) return;
    this.gonderiliyor = true;
    this.hata = '';
    const payload = {
      ziyaretEdenAdSoyad: this.ziyaretEdenAdSoyad.trim(),
      unvan: this.unvan,
      baslangicTarihi: new Date(this.baslangicTarihi).toISOString(),
      bitisTarihi: new Date(this.bitisTarihi).toISOString(),
      il: this.il,
      mekan: this.mekan.trim(),
      hassasiyet: +this.hassasiyet,
      guvenlikSeviyesi: this.guvenlikSeviyesi,
      gozlemNoktalari: this.gozlemNoktalari.trim(),
      ziyaretDurumu: 3
    };
    this.http.put<VIPZiyaretRecord>(`${this.apiBase}/vipziyaret/${this.selectedPlanId}`, payload, { headers: this.getHeaders() })
      .subscribe({
        next: (guncellenen) => {
          const idx = this.kayitlar.findIndex(k => k.id === this.selectedPlanId);
          if (idx !== -1) this.kayitlar[idx] = guncellenen;
          this.basari = 'Ziyaret iptal olarak işaretlendi.';
          this.gonderiliyor = false;
          this.switchToPlannedForm();
          this.cdr.markForCheck();
        },
        error: () => { this.hata = 'İptal işlemi başarısız.'; this.gonderiliyor = false; this.cdr.markForCheck(); }
      });
  }

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
        next: (data) => { this.kayitlar = data; this.yukleniyor = false; this.cdr.markForCheck(); },
        error: () => { this.hata = 'Kayitlar yuklenemedi.'; this.yukleniyor = false; this.cdr.markForCheck(); }
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
      hassasiyet: +this.hassasiyet,
      guvenlikSeviyesi: this.guvenlikSeviyesi,
      gozlemNoktalari: this.gozlemNoktalari.trim(),
      ziyaretDurumu: +this.ziyaretDurumu
    };

    if (this.selectedPlanId) {
      // Planlananı gerçekleşen olarak güncelle
      this.http.put<VIPZiyaretRecord>(`${this.apiBase}/vipziyaret/${this.selectedPlanId}`, payload, { headers: this.getHeaders() })
        .subscribe({
          next: (guncellenen) => {
            const idx = this.kayitlar.findIndex(k => k.id === this.selectedPlanId);
            if (idx !== -1) this.kayitlar[idx] = guncellenen;
            this.takipNo = guncellenen.takipNo ?? null;
            this.basari = 'Ziyaret gerçekleşti olarak kaydedildi.';
            this.gonderiliyor = false;
            this.switchToPlannedForm();
            this.cdr.markForCheck();
          },
          error: (err) => {
            this.hata = err.error?.message || 'Güncelleme sırasında hata oluştu.';
            this.gonderiliyor = false;
            this.cdr.markForCheck();
          }
        });
    } else {
      this.http.post<VIPZiyaretRecord>(`${this.apiBase}/vipziyaret`, payload, { headers: this.getHeaders() })
        .subscribe({
          next: (yeni) => {
            this.kayitlar.unshift(yeni);
            this.takipNo = yeni.takipNo ?? null;
            this.formuSifirla();
            this.basari = 'Ziyaret kaydi basariyla olusturuldu.';
            this.gonderiliyor = false;
            this.cdr.markForCheck();
          },
          error: (err) => {
            this.hata = err.error?.message || 'Kayit sirasinda hata olustu.';
            this.gonderiliyor = false;
            this.cdr.markForCheck();
          }
        });
    }
  }



  private formuSifirla(): void {
    this.ziyaretEdenAdSoyad = '';
    this.unvan = '';
    this.mekan = '';
    this.il = '';
    this.ziyaretDurumu = 0;
    this.hassasiyet = 0;
    this.guvenlikSeviyesi = 'Normal';
    this.gozlemNoktalari = '';
    this.baslangicTarihi = new Date().toISOString().substring(0, 16);
    this.bitisTarihi = new Date(Date.now() + 60 * 60 * 1000).toISOString().substring(0, 16);
  }

  trackById(_index: number, item: { id: string }): string { return item.id; }
}