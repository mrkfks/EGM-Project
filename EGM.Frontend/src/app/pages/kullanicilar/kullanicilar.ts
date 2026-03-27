import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface UserKaydi {
  id: string;
  sicil: number;
  role: string;
  fullName: string;
  email: string;
  gsm: string;
}

@Component({
  selector: 'app-kullanicilar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './kullanicilar.html',
  styleUrls: ['./kullanicilar.css'],
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

  rolModalAcik = false;
  secilenKullanici: UserKaydi | null = null;
  yeniRol = 'Izleyici';
  cityIdInput: number | null = null;
  rolAtiyor = false;

  silModalAcik = false;
  silinecekKullanici: UserKaydi | null = null;
  siliyor = false;

  formSicil: number | null = null;
  formSifre = '';
  formAdSoyad = '';
  formEmail = '';
  formGsm = '';
  formCityId: number | null = null;
  ekleniyor = false;

  mevcutRol = '';

  readonly roller = [
    { value: 'Izleyici',            label: 'Izleyici' },
    { value: 'IlPersoneli',         label: 'Il Personeli' },
    { value: 'IlYoneticisi',        label: 'Il Yoneticisi' },
    { value: 'BaskanlikPersoneli',  label: 'Baskanlik Personeli' },
    { value: 'BaskanlikYoneticisi', label: 'Baskanlik Yoneticisi' },
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

  constructor(private http: HttpClient) {}

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
        this.yukleniyor = false;
      },
      error: (err) => {
        this.hataMesaji = err?.error?.toString()?.substring(0, 120) ?? 'Kullanicilar yuklenemedi. Backend baglantisi kontrol edin.';
        this.yukleniyor = false;
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
      cityId: this.formCityId,
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
      },
    });
  }

  private formuSifirla(): void {
    this.formSicil = null; this.formSifre = '';
    this.formAdSoyad = ''; this.formEmail = '';
    this.formGsm = ''; this.formCityId = null;
  }

  // ── Rol atama ──────────────────────────────────────────────────────────
  rolModalAc(k: UserKaydi): void {
    this.secilenKullanici = k;
    this.yeniRol = k.role;
    this.cityIdInput = null;
    this.rolModalAcik = true;
    this.hataMesaji = '';
    this.basariMesaji = '';
  }

  rolAta(): void {
    if (!this.secilenKullanici) return;
    this.rolAtiyor = true;
    this.http.post(
      `${this.apiUser}/${this.secilenKullanici.sicil}/rol-ata`,
      { yeniRol: this.yeniRol, cityId: this.cityIdInput }
    ).subscribe({
      next: () => {
        this.basariMesaji = `${this.secilenKullanici!.fullName} icin rol guncellendi.`;
        this.rolAtiyor = false;
        this.rolModalAcik = false;
        this.secilenKullanici = null;
        this.kullanicilariGetir();
      },
      error: (err) => {
        this.hataMesaji = err?.error?.mesaj ?? err?.error ?? 'Rol atanamadi.';
        this.rolAtiyor = false;
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
      },
    });
  }

  modalKapat(): void {
    this.rolModalAcik = false;
    this.silModalAcik = false;
    this.secilenKullanici = null;
    this.silinecekKullanici = null;
  }

  alertKapat(): void { this.hataMesaji = ''; this.basariMesaji = ''; }

  get baskanlikYoneticisi(): boolean { return this.mevcutRol === 'BaskanlikYoneticisi'; }
  get ilYoneticisiVeyaUstu(): boolean {
    return ['IlYoneticisi', 'BaskanlikPersoneli', 'BaskanlikYoneticisi'].includes(this.mevcutRol);
  }

  rolRenk(rol: string): { bg: string; color: string; border: string } {
    switch (rol) {
      case 'Izleyici':             return { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' };
      case 'IlPersoneli':         return { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' };
      case 'IlYoneticisi':        return { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' };
      case 'BaskanlikPersoneli':  return { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' };
      case 'BaskanlikYoneticisi': return { bg: '#fef9e7', color: '#d68910', border: '#f9e79f' };
      default:                    return { bg: '#f8f9fa', color: '#495057', border: '#dee2e6' };
    }
  }

  rolEtiket(rol: string): string {
    const buldu = this.roller.find(r => r.value === rol);
    return buldu ? buldu.label : rol;
  }

  rolSayisi(rol: string): number {
    return this.kullanicilar.filter(k => k.role === rol).length;
  }
}