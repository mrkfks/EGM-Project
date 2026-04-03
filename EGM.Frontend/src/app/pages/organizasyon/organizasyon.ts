import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

interface KurulusKaydi {
  id: string;
  ad: string;
  kurulusTarihi: string;
  faaliyetAlani: string;
  iletisim: string;
  tur: string;
  aciklama: string;
  ustKurulusId: string | null;
  ustKurulusAd: string | null;
  altKurulusSayisi: number;
  telefon?: string;
  eposta?: string;
  sosyalMedyaHesaplari?: string;
  siyasiYonelim?: string;
  kutukNumarasi?: string;
  logo?: string | null;
}

@Component({
  selector: 'app-organizasyon',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './organizasyon.html',
  styleUrls: ['./organizasyon.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Organizasyon implements OnInit {
  private readonly apiBase = `${environment.apiUrl}/api/organizator`;

  tumKuruluslar: KurulusKaydi[] = [];
  yukleniyor = false;
  hataMesaji = '';
  basariMesaji = '';

  // Form
  formAd = '';
  formTur = 'Sendika';
  formFaaliyetAlani = '';
  formIletisim = '';
  formAciklama = '';
  formKurulusTarihi = '';
  formUstKurulusId: string | null = null;
  formLogo = '';
  formTelefon = '';
  formEposta = '';
  formSosyalMedya: { platform: string; hesap: string }[] = [];
  yeniSosPlatform = '';
  yeniSosHesap = '';
  formSiyasiYonelim = '';
  formKutukNumarasi = '';
  ekleniyor = false;

  readonly sosyalPlatformlar = [
    'Twitter / X', 'Facebook', 'Instagram', 'YouTube',
    'TikTok', 'Telegram', 'LinkedIn', 'WhatsApp', 'Diğer',
  ];

  private readonly STORAGE_KEY = 'egm_kurulus_turleri';
  private readonly YONELIM_STORAGE_KEY = 'egm_kurulus_yonelimleri';
  private readonly LOGO_STORAGE_KEY = 'egm_kurulus_logolar';
  private readonly ORG_LOGO_KEY = 'egm_org_logolar';
  private readonly VARSAYILAN_TURLER = [
    'Konfederasyon', 'Sendika', 'Siyasi Parti', 'Dernek', 'Vakif', 'STK', 'Kamu Kurumu', 'Diger',
  ];
  private readonly VARSAYILAN_YONELIMLER = [
    'Sol / Sosyalist', 'Merkez Sol', 'Merkez', 'Merkez Sag', 'Sag / Milliyetci', 'Liberal', 'Muhafazakar', 'Bagimsiz',
  ];
  private readonly RENK_PALETI = [
    { bg: '#fef9e7', color: '#d68910', border: '#f9e79f' },
    { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' },
    { bg: '#fdecea', color: '#c0392b', border: '#f1948a' },
    { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' },
    { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' },
    { bg: '#eafaf1', color: '#1e8449', border: '#a9dfbf' },
    { bg: '#eaf2ff', color: '#1a5276', border: '#a9cce3' },
    { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' },
    { bg: '#fef0e6', color: '#ca6f1e', border: '#f0b27a' },
    { bg: '#f9ebea', color: '#922b21', border: '#f1948a' },
  ];

  turler: string[] = [];
  turLogolari: Record<string, string> = {};
  orgLogolari: Record<string, string> = {};
  secilenLogoOrgId: string = '';
  yeniTur = '';
  yeniTurAcik = false;

  yonelimler: string[] = [];
  yeniYonelim = '';
  yeniYonelimAcik = false;

  readonly yonelimRenkleri: Record<string, { bg: string; color: string; border: string }> = {
    'Sol / Sosyalist':  { bg: '#fde8e8', color: '#c0392b', border: '#f5b7b1' },
    'Merkez Sol':       { bg: '#fef0f0', color: '#e74c3c', border: '#fadbd8' },
    'Merkez':           { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' },
    'Merkez Sag':       { bg: '#eaf2ff', color: '#2980b9', border: '#aed6f1' },
    'Sag / Milliyetci': { bg: '#ebf5fb', color: '#1a5276', border: '#85c1e9' },
    'Liberal':          { bg: '#fef9e7', color: '#b7950b', border: '#f9e79f' },
    'Muhafazakar':      { bg: '#eafaf1', color: '#1e8449', border: '#a9dfbf' },
    'Bagimsiz':         { bg: '#f8f9fa', color: '#6c757d', border: '#dee2e6' },
  };

  readonly turRenkleri: Record<string, { bg: string; color: string; border: string }> = {
    'Konfederasyon': { bg: '#fef9e7', color: '#d68910', border: '#f9e79f' },
    'Sendika':       { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' },
    'Siyasi Parti':  { bg: '#fdecea', color: '#c0392b', border: '#f1948a' },
    'Dernek':        { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' },
    'Vakif':         { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' },
    'STK':           { bg: '#eafaf1', color: '#1e8449', border: '#a9dfbf' },
    'Kamu Kurumu':   { bg: '#eaf2ff', color: '#1a5276', border: '#a9cce3' },
    'Diger':         { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' },
  };

  constructor(private http: HttpClient, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.turleriYukle();
    this.yonelimleriYukle();
    this.logolariYukle();
    this.orgLogolariYukle();
    this.kuruluslariGetir();
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  kuruluslariGetir(): void {
    this.yukleniyor = true;
    this.http.get<KurulusKaydi[]>(this.apiBase, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        this.tumKuruluslar = data;
        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.yukleniyor = false;
        this.cdr.markForCheck();
      },
    });
  }

  get ustDuzeyKuruluslar(): KurulusKaydi[] {
    return this.tumKuruluslar.filter(k => !k.ustKurulusId);
  }

  altKuruluslariGetir(ustId: string): KurulusKaydi[] {
    return this.tumKuruluslar.filter(k => k.ustKurulusId === ustId);
  }

  formGecerliMi(): boolean {
    return !!(this.formAd.trim() && this.formTur && this.formKurulusTarihi);
  }

  kurulusEkle(): void {
    if (!this.formGecerliMi()) return;
    this.ekleniyor = true;
    this.hataMesaji = '';
    this.basariMesaji = '';
    const body = {
      ad: this.formAd.trim(),
      tur: this.formTur,
      faaliyetAlani: this.formFaaliyetAlani.trim(),
      iletisim: this.formIletisim.trim(),
      aciklama: this.formAciklama.trim(),
      kurulusTarihi: this.formKurulusTarihi,
      ustKurulusId: this.formUstKurulusId || null,
      telefon: this.formTelefon.trim() || null,
      eposta: this.formEposta.trim() || null,
      sosyalMedyaHesaplari: this.formSosyalMedya.length
        ? JSON.stringify(this.formSosyalMedya)
        : null,
      siyasiYonelim: this.formSiyasiYonelim || null,
      kutukNumarasi: this.formKutukNumarasi.trim() || null,
      logo: this.formLogo || null,
    };
    this.http.post<KurulusKaydi>(this.apiBase, body, { headers: this.getHeaders() }).subscribe({
      next: (yeni) => {
        this.ekleniyor = false;
        this.formuSifirla();
        this.router.navigate(['/rapor-kuruluslar'], { queryParams: { id: yeni.id, mesaj: 'kayit' } });
      },
      error: (err) => {
        const e = err?.error;
        this.hataMesaji = typeof e === 'string' ? e : (e?.title ?? e?.message ?? `Hata ${err?.status ?? ''}: Kurulus eklenemedi.`.trim());
        this.ekleniyor = false;
        this.cdr.markForCheck();
      },
    });
  }

  private formuSifirla(): void {
    this.formAd = '';
    this.formTur = this.turler[0] ?? '';
    this.formFaaliyetAlani = '';
    this.formIletisim = '';
    this.formAciklama = '';
    this.formKurulusTarihi = '';
    this.formUstKurulusId = null;
    this.formLogo = '';
    this.formTelefon = '';
    this.formEposta = '';
    this.formSosyalMedya = [];
    this.yeniSosPlatform = '';
    this.yeniSosHesap = '';
    this.formSiyasiYonelim = '';
    this.formKutukNumarasi = '';
  }

  sosyalMedyaEkle(): void {
    const p = this.yeniSosPlatform.trim();
    const h = this.yeniSosHesap.trim();
    if (!p || !h) return;
    this.formSosyalMedya = [...this.formSosyalMedya, { platform: p, hesap: h }];
    this.yeniSosPlatform = '';
    this.yeniSosHesap = '';
  }

  sosyalMedyaSil(index: number): void {
    this.formSosyalMedya = this.formSosyalMedya.filter((_, i) => i !== index);
  }

  formLogoSec(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      this.hataMesaji = 'Logo dosyası 5 MB\'dan küçük olmalıdır.';
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => { this.formLogo = e.target!.result as string; };
    reader.readAsDataURL(file);
    input.value = '';
  }

  formLogoSil(): void {
    this.formLogo = '';
  }

  alertKapat(): void {
    this.hataMesaji = '';
    this.basariMesaji = '';
  }

  turRenk(tur: string) {
    if (this.turRenkleri[tur]) return this.turRenkleri[tur];
    const idx = this.turler.indexOf(tur);
    return this.RENK_PALETI[idx % this.RENK_PALETI.length] ?? this.turRenkleri['Diger'];
  }

  private turleriYukle(): void {
    const kayitli = localStorage.getItem(this.STORAGE_KEY);
    if (kayitli) {
      try { this.turler = JSON.parse(kayitli); } catch { this.turler = [...this.VARSAYILAN_TURLER]; }
    } else {
      this.turler = [...this.VARSAYILAN_TURLER];
    }
    if (!this.turler.includes(this.formTur)) {
      this.formTur = this.turler[0] ?? '';
    }
  }

  private turleriKaydet(): void {
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.turler));
  }

  private yonelimleriYukle(): void {
    const kayitli = localStorage.getItem(this.YONELIM_STORAGE_KEY);
    if (kayitli) {
      try { this.yonelimler = JSON.parse(kayitli); } catch { this.yonelimler = [...this.VARSAYILAN_YONELIMLER]; }
    } else {
      this.yonelimler = [...this.VARSAYILAN_YONELIMLER];
    }
  }

  private yonelimleriKaydet(): void {
    localStorage.setItem(this.YONELIM_STORAGE_KEY, JSON.stringify(this.yonelimler));
  }

  yonelimSec(y: string): void {
    this.formSiyasiYonelim = this.formSiyasiYonelim === y ? '' : y;
  }

  yonelimRenk(y: string) {
    if (this.yonelimRenkleri[y]) return this.yonelimRenkleri[y];
    const idx = this.yonelimler.indexOf(y);
    return this.RENK_PALETI[idx % this.RENK_PALETI.length] ?? this.yonelimRenkleri['Bagimsiz'];
  }

  yonelimEkleInline(): void {
    const y = this.yeniYonelim.trim();
    if (!y || this.yonelimler.includes(y)) return;
    this.yonelimler = [...this.yonelimler, y];
    this.yonelimleriKaydet();
    this.formSiyasiYonelim = y;
    this.yeniYonelim = '';
    this.yeniYonelimAcik = false;
  }

  yonelimSil(y: string): void {
    this.yonelimler = this.yonelimler.filter(v => v !== y);
    if (this.formSiyasiYonelim === y) this.formSiyasiYonelim = '';
    this.yonelimleriKaydet();
  }

  private logolariYukle(): void {
    const kayitli = localStorage.getItem(this.LOGO_STORAGE_KEY);
    if (kayitli) {
      try { this.turLogolari = JSON.parse(kayitli); } catch { this.turLogolari = {}; }
    }
  }

  private logolariKaydet(): void {
    localStorage.setItem(this.LOGO_STORAGE_KEY, JSON.stringify(this.turLogolari));
  }

  logoInputTikla(tur: string): void {
    const id = 'tur-logo-' + tur.replace(/\s/g, '_');
    const input = document.getElementById(id) as HTMLInputElement;
    if (input) input.click();
  }

  logoYukle(tur: string, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.size > 1024 * 1024) {
      this.hataMesaji = 'Logo dosyası 1 MB\'dan küçük olmalıdır.';
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      this.turLogolari = { ...this.turLogolari, [tur]: e.target!.result as string };
      this.logolariKaydet();
      input.value = '';
    };
    reader.readAsDataURL(file);
  }

  logoSil(tur: string, event: Event): void {
    event.stopPropagation();
    const { [tur]: _, ...rest } = this.turLogolari;
    this.turLogolari = rest as Record<string, string>;
    this.logolariKaydet();
  }

  // ── Kuruluş logoları (ID bazlı) ──────────────────────────────────
  private orgLogolariYukle(): void {
    const kayitli = localStorage.getItem(this.ORG_LOGO_KEY);
    if (kayitli) {
      try { this.orgLogolari = JSON.parse(kayitli); } catch { this.orgLogolari = {}; }
    }
  }

  orgLogoInputTikla(orgId: string): void {
    const input = document.getElementById('org-logo-' + orgId) as HTMLInputElement;
    if (input) input.click();
  }

  orgLogoYukle(orgId: string, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.size > 2 * 1024 * 1024) {
      this.hataMesaji = 'Logo dosyası 2 MB\'dan küçük olmalıdır.';
      return;
    }
    const reader = new FileReader();
    reader.onload = (e) => {
      this.orgLogolari = { ...this.orgLogolari, [orgId]: e.target!.result as string };
      localStorage.setItem(this.ORG_LOGO_KEY, JSON.stringify(this.orgLogolari));
      input.value = '';
    };
    reader.readAsDataURL(file);
  }

  orgLogoSil(orgId: string, event: Event): void {
    event.stopPropagation();
    const { [orgId]: _, ...rest } = this.orgLogolari;
    this.orgLogolari = rest as Record<string, string>;
    localStorage.setItem(this.ORG_LOGO_KEY, JSON.stringify(this.orgLogolari));
  }

  turEkle(): void {
    const tur = this.yeniTur.trim();
    if (!tur || this.turler.includes(tur)) return;
    this.turler = [...this.turler, tur];
    this.turleriKaydet();
    this.yeniTur = '';
  }

  turEkleInline(): void {
    const tur = this.yeniTur.trim();
    if (!tur || this.turler.includes(tur)) return;
    this.turler = [...this.turler, tur];
    this.turleriKaydet();
    this.formTur = tur;
    this.yeniTur = '';
    this.yeniTurAcik = false;
  }

  turSil(tur: string): void {
    if (this.turKullanimi(tur) > 0) {
      this.hataMesaji = `"${tur}" türü kullanımda olduğu için silinemez.`;
      return;
    }
    this.turler = this.turler.filter(t => t !== tur);
    if (this.formTur === tur) this.formTur = this.turler[0] ?? '';
    this.turleriKaydet();
  }

  turKullanimi(tur: string): number {
    return this.tumKuruluslar.filter(k => k.tur === tur).length;
  }

  tarihFormat(tarih: string): string {
    if (!tarih) return '-';
    try { return new Date(tarih).toLocaleDateString('tr-TR'); } catch { return tarih; }
  }
}
