import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface UserKaydi {
  id: string;
  sicil: number;
  role: string;
  fullName: string;
  email: string;
  gsm: string;
  cityId: number | null;
  birim: string;
}

@Component({
  selector: 'app-kullanicilar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './kullanicilar.html',
  styleUrls: ['./kullanicilar.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Kullanicilar implements OnInit {
  private readonly apiUser = 'http://localhost:5117/api/user';
  private readonly apiRegister = 'http://localhost:5117/api/auth/register';

  aktifSekme: 'liste' | 'ekle' = 'liste';

  kullanicilar: UserKaydi[] = [];
  filtrelenmis: UserKaydi[] = [];
  aramaMetni = '';
  yukleniyor = false;

  hataMesaji = '';
  basariMesaji = '';

  duzenleModalAcik = false;
  secilenKullanici: UserKaydi | null = null;
  duzenleFullName = '';
  duzenleEmail = '';
  duzenleGsm = '';
  dunzleBirim = '';
  duzenleRol = 'Izleyici';
  dunzeleCityId: number | null = null;
  duzenleYapiyor = false;

  silModalAcik = false;
  silinecekKullanici: UserKaydi | null = null;
  siliyor = false;

  formSicil: number | null = null;
  formSifre = '';
  formAdSoyad = '';
  formEmail = '';
  formGsm = '';
  formBirim = '';
  formCityId: number | null = null;
  formRol = 'Izleyici';
  ekleniyor = false;

  mevcutRol = '';

  readonly roller = [
    { value: 'Izleyici',            label: 'Izleyici' },
    { value: 'IlPersoneli',         label: 'Il Personeli' },
    { value: 'IlYoneticisi',        label: 'Il Yoneticisi' },
    { value: 'BaskanlikPersoneli',  label: 'Baskanlik Personeli' },
    { value: 'BaskanlikYoneticisi', label: 'Baskanlik Yoneticisi' },
    { value: 'Yetkili',             label: 'Yetkili' },
  ];

  readonly iller: { kod: number; ad: string }[] = [
    { kod: 1, ad: 'Adana' }, { kod: 2, ad: 'Adiyaman' }, { kod: 3, ad: 'Afyonkarahisar' },
    { kod: 4, ad: 'Agri' }, { kod: 5, ad: 'Amasya' }, { kod: 6, ad: 'Ankara' },
    { kod: 7, ad: 'Antalya' }, { kod: 8, ad: 'Artvin' }, { kod: 9, ad: 'Aydin' },
    { kod: 10, ad: 'Balikesir' }, { kod: 11, ad: 'Bilecik' }, { kod: 12, ad: 'Bingol' },
    { kod: 13, ad: 'Bitlis' }, { kod: 14, ad: 'Bolu' }, { kod: 15, ad: 'Burdur' },
    { kod: 16, ad: 'Bursa' }, { kod: 17, ad: 'Canakkale' }, { kod: 18, ad: 'Cankiri' },
    { kod: 19, ad: 'Corum' }, { kod: 20, ad: 'Denizli' }, { kod: 21, ad: 'Diyarbakir' },
    { kod: 22, ad: 'Edirne' }, { kod: 23, ad: 'Elazig' }, { kod: 24, ad: 'Erzincan' },
    { kod: 25, ad: 'Erzurum' }, { kod: 26, ad: 'Eskisehir' }, { kod: 27, ad: 'Gaziantep' },
    { kod: 28, ad: 'Giresun' }, { kod: 29, ad: 'Gumushane' }, { kod: 30, ad: 'Hakkari' },
    { kod: 31, ad: 'Hatay' }, { kod: 32, ad: 'Isparta' }, { kod: 33, ad: 'Mersin' },
    { kod: 34, ad: 'Istanbul' }, { kod: 35, ad: 'Izmir' }, { kod: 36, ad: 'Kars' },
    { kod: 37, ad: 'Kastamonu' }, { kod: 38, ad: 'Kayseri' }, { kod: 39, ad: 'Kirklareli' },
    { kod: 40, ad: 'Kirsehir' }, { kod: 41, ad: 'Kocaeli' }, { kod: 42, ad: 'Konya' },
    { kod: 43, ad: 'Kutahya' }, { kod: 44, ad: 'Malatya' }, { kod: 45, ad: 'Manisa' },
    { kod: 46, ad: 'Kahramanmaras' }, { kod: 47, ad: 'Mardin' }, { kod: 48, ad: 'Mugla' },
    { kod: 49, ad: 'Mus' }, { kod: 50, ad: 'Nevsehir' }, { kod: 51, ad: 'Nigde' },
    { kod: 52, ad: 'Ordu' }, { kod: 53, ad: 'Rize' }, { kod: 54, ad: 'Sakarya' },
    { kod: 55, ad: 'Samsun' }, { kod: 56, ad: 'Siirt' }, { kod: 57, ad: 'Sinop' },
    { kod: 58, ad: 'Sivas' }, { kod: 59, ad: 'Tekirdag' }, { kod: 60, ad: 'Tokat' },
    { kod: 61, ad: 'Trabzon' }, { kod: 62, ad: 'Tunceli' }, { kod: 63, ad: 'Sanliurfa' },
    { kod: 64, ad: 'Usak' }, { kod: 65, ad: 'Van' }, { kod: 66, ad: 'Yozgat' },
    { kod: 67, ad: 'Zonguldak' }, { kod: 68, ad: 'Aksaray' }, { kod: 69, ad: 'Bayburt' },
    { kod: 70, ad: 'Karaman' }, { kod: 71, ad: 'Kirikkale' }, { kod: 72, ad: 'Batman' },
    { kod: 73, ad: 'Sirnak' }, { kod: 74, ad: 'Bartin' }, { kod: 75, ad: 'Ardahan' },
    { kod: 76, ad: 'Igdir' }, { kod: 77, ad: 'Yalova' }, { kod: 78, ad: 'Karabuk' },
    { kod: 79, ad: 'Kilis' }, { kod: 80, ad: 'Osmaniye' }, { kod: 81, ad: 'Duzce' },
  ];

  readonly birimler: string[] = [
    'Başkanlık',
    'Adana', 'Adıyaman', 'Afyonkarahisar', 'Ağrı', 'Amasya', 'Ankara', 'Antalya', 'Artvin',
    'Aydın', 'Balıkesir', 'Bilecik', 'Bingöl', 'Bitlis', 'Bolu', 'Burdur', 'Bursa',
    'Çanakkale', 'Çankırı', 'Çorum', 'Denizli', 'Diyarbakır', 'Edirne', 'Elazığ', 'Erzincan',
    'Erzurum', 'Eskişehir', 'Gaziantep', 'Giresun', 'Gümüşhane', 'Hakkari', 'Hatay', 'Isparta',
    'Mersin', 'İstanbul', 'İzmir', 'Kars', 'Kastamonu', 'Kayseri', 'Kırklareli', 'Kırşehir',
    'Kocaeli', 'Konya', 'Kütahya', 'Malatya', 'Manisa', 'Kahramanmaraş', 'Mardin', 'Muğla',
    'Muş', 'Nevşehir', 'Niğde', 'Ordu', 'Rize', 'Sakarya', 'Samsun', 'Siirt', 'Sinop',
    'Sivas', 'Tekirdağ', 'Tokat', 'Trabzon', 'Tunceli', 'Şanlıurfa', 'Uşak', 'Van', 'Yozgat',
    'Zonguldak', 'Aksaray', 'Bayburt', 'Karaman', 'Kırıkkale', 'Batman', 'Şırnak', 'Bartın',
    'Ardahan', 'Iğdır', 'Yalova', 'Karabük', 'Kilis', 'Osmaniye', 'Düzce',
  ];

  yetkiliSayisi = 0;

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.jwtOku();
    this.kullanicilariGetir();
  }

  private jwtOku(): void {
    try {
      const token = localStorage.getItem('token');
      if (!token) return;
      const payload = JSON.parse(atob(token.split('.')[1]));
      this.mevcutRol =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        ?? payload['role'] ?? payload['Role'] ?? '';
    } catch {}
  }

  kullanicilariGetir(): void {
    this.yukleniyor = true;
    this.hataMesaji = '';
    this.http.get<UserKaydi[]>(this.apiUser).subscribe({
      next: (data) => {
        this.kullanicilar = data;
        this.filtrele();
        this.yetkiliSayisi = this.kullanicilar.filter(k => k.role === 'Yetkili').length; // Calculate Yetkili count
        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.hataMesaji = err?.error?.toString()?.substring(0, 120) ?? 'Kullanicilar yuklenemedi. Backend baglantisi kontrol edin.';
        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
    });
  }

  filtrele(): void {
    const ara = this.aramaMetni.toLowerCase().trim();
    this.filtrelenmis = ara
      ? this.kullanicilar.filter(k =>
          k.fullName.toLowerCase().includes(ara) ||
          k.email.toLowerCase().includes(ara) ||
          String(k.sicil).includes(ara) ||
          k.role.toLowerCase().includes(ara))
      : [...this.kullanicilar];
  }

  // ── Yeni kullanici ─────────────────────────────────────────────────────
  formGecerliMi(): boolean {
    return !!(this.formSicil && this.formSifre.length >= 8 &&
      this.formAdSoyad.trim() && this.formEmail.trim());
  }

  kullaniciEkle(): void {
    if (!this.formGecerliMi()) return;
    this.ekleniyor = true;
    this.hataMesaji = '';
    this.basariMesaji = '';
    const body = {
      sicil: this.formSicil,
      password: this.formSifre,
      fullName: this.formAdSoyad.trim(),
      email: this.formEmail.trim(),
      gsm: this.formGsm.trim(),
      birim: this.formBirim.trim(),
      cityId: this.formCityId,
      role: this.formRol || 'Izleyici',
    };
    this.http.post(this.apiRegister, body, { responseType: 'text' }).subscribe({
      next: () => {
        this.basariMesaji = 'Kullanici basariyla eklendi.';
        this.ekleniyor = false;
        this.formuSifirla();
        this.aktifSekme = 'liste';
        this.kullanicilariGetir();
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Kullanici eklenemedi.';
        this.ekleniyor = false;
        this.cdr.markForCheck();
      },
    });
  }

  private formuSifirla(): void {
    this.formSicil = null; this.formSifre = '';
    this.formAdSoyad = ''; this.formEmail = '';
    this.formGsm = ''; this.formBirim = ''; this.formCityId = null; this.formRol = 'Izleyici';
  }

  // ── Düzenleme ──────────────────────────────────────────────────────────
  duzenleModalAc(k: UserKaydi): void {
    this.secilenKullanici = k;
    this.duzenleFullName = k.fullName;
    this.duzenleEmail = k.email;
    this.duzenleGsm = k.gsm;
    this.dunzleBirim = k.birim;
    this.duzenleRol = k.role;
    this.birimDegistiDuzenle();
    this.dunzeleCityId = k.cityId;
    this.duzenleModalAcik = true;
    this.hataMesaji = '';
    this.basariMesaji = '';
  }

  duzenleKaydet(): void {
    if (!this.secilenKullanici) return;
    this.duzenleYapiyor = true;
    const sicil = this.secilenKullanici.sicil;
    const ad    = this.secilenKullanici.fullName;
    const body = {
      fullName: this.duzenleFullName.trim(),
      email:    this.duzenleEmail.trim(),
      gsm:      this.duzenleGsm.trim(),
      birim:    this.dunzleBirim.trim(),
      role:     this.duzenleRol,
      cityId:   this.dunzeleCityId,
    };
    this.http.put(`${this.apiUser}/${sicil}`, body, { responseType: 'json' }).subscribe({
      next: () => {
        this.duzenleYapiyor   = false;
        this.duzenleModalAcik = false;
        this.secilenKullanici = null;
        this.basariMesaji     = `${ad} basariyla guncellendi.`;
        this.kullanicilariGetir();
      },
      error: (err) => {
        this.duzenleYapiyor = false;
        this.hataMesaji = err?.error?.mesaj ?? err?.error ?? 'Kullanici guncellenemedi.';
        this.cdr.markForCheck();
      },
    });
  }

  // ── Silme ──────────────────────────────────────────────────────────────
  silModalAc(k: UserKaydi): void {
    this.silinecekKullanici = k;
    this.silModalAcik = true;
  }

  kullaniciSil(): void {
    if (!this.silinecekKullanici) return;
    this.siliyor = true;
    this.http.delete(
      `${this.apiUser}/${this.silinecekKullanici.sicil}`,
      { responseType: 'text' }
    ).subscribe({
      next: () => {
        this.basariMesaji = 'Kullanici silindi.';
        this.siliyor = false;
        this.silModalAcik = false;
        this.silinecekKullanici = null;
        this.kullanicilariGetir();
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Kullanici silinemedi.';
        this.siliyor = false;
        this.silModalAcik = false;
        this.cdr.markForCheck();
      },
    });
  }

  modalKapat(): void {
    this.duzenleModalAcik = false;
    this.duzenleYapiyor   = false;
    this.silModalAcik     = false;
    this.secilenKullanici = null;
    this.silinecekKullanici = null;
  }

  alertKapat(): void { this.hataMesaji = ''; this.basariMesaji = ''; }

  get baskanlikYoneticisi(): boolean { return ['BaskanlikYoneticisi', 'Yetkili'].includes(this.mevcutRol); }
  get ilYoneticisiVeyaUstu(): boolean {
    return ['IlYoneticisi', 'BaskanlikPersoneli', 'BaskanlikYoneticisi', 'Yetkili'].includes(this.mevcutRol);
  }

  rolRenk(rol: string): { bg: string; color: string; border: string } {
    switch (rol) {
      case 'Izleyici':             return { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' };
      case 'IlPersoneli':         return { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' };
      case 'IlYoneticisi':        return { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' };
      case 'BaskanlikPersoneli':  return { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' };
      case 'BaskanlikYoneticisi': return { bg: '#fef9e7', color: '#d68910', border: '#f9e79f' };
      case 'Yetkili':             return { bg: '#fdf2f8', color: '#c0392b', border: '#f1948a' };
      default:                    return { bg: '#f8f9fa', color: '#495057', border: '#dee2e6' };
    }
  }

  rolEtiket(rol: string): string {
    const buldu = this.roller.find(r => r.value === rol);
    return buldu ? buldu.label : rol;
  }

  // Hiyerarşi sırası
  private readonly rolSirasi = [
    'Izleyici', 'IlPersoneli', 'IlYoneticisi',
    'BaskanlikPersoneli', 'BaskanlikYoneticisi', 'Yetkili'
  ];

  /**
   * Mevcut kullanıcının atayabileceği rollerin listesi.
   * İl Yöneticisi: yalnızca IlPersoneli
   * Başkanlık Yöneticisi: kendi altındaki roller
   * Yetkili: tüm roller
   */
  get atanabilirRoller(): { value: string; label: string }[] {
    if (this.mevcutRol === 'Yetkili') return this.roller;
    const mevcutIndex = this.rolSirasi.indexOf(this.mevcutRol);
    if (this.mevcutRol === 'IlYoneticisi') {
      return this.roller.filter(r => r.value === 'IlPersoneli');
    }
    return this.roller.filter(r => {
      const idx = this.rolSirasi.indexOf(r.value);
      return idx >= 0 && idx < mevcutIndex;
    });
  }

  rolSayisi(rol: string): number {
    return this.kullanicilar.filter(k => k.role === rol).length;
  }

  // ── Birime göre uygun rol değerleri ─────────────────────────────
  private birimUygunRolDegerleri(birim: string): string[] {
    if (birim === 'Başkanlık') {
      return ['Izleyici', 'BaskanlikPersoneli', 'BaskanlikYoneticisi', 'Yetkili'];
    }
    if (birim) {
      // 81 il
      return ['Izleyici', 'IlPersoneli', 'IlYoneticisi', 'Yetkili'];
    }
    // birim seçilmemiş - tüm roller
    return this.rolSirasi.slice();
  }

  get duzenleAtanabilirRoller(): { value: string; label: string }[] {
    const uygun = this.birimUygunRolDegerleri(this.dunzleBirim);
    return this.atanabilirRoller.filter(r => uygun.includes(r.value));
  }

  birimDegistiDuzenle(): void {
    const izinliler = this.duzenleAtanabilirRoller;
    if (!izinliler.find(r => r.value === this.duzenleRol)) {
      this.duzenleRol = izinliler[0]?.value ?? 'Izleyici';
    }
  }
}