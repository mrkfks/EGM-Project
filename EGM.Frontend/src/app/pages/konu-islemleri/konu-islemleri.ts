import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { timer, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

interface KategoriKaydi {
  id: string;
  ad: string | null;
}

interface KonuKaydi {
  id: string;
  ad: string;
  aciklama: string | null;
  tur: string | null;
  ustKonuId: string | null;
  ustKonuAd: string | null;
  altKonuSayisi: number;
  createdAt: string;
}

@Component({
  selector: 'app-konu-islemleri',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './konu-islemleri.html',
  styleUrls: ['./konu-islemleri.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class KonuIslemleri implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly apiRoot = `${environment.apiUrl}/api/organizator`;
  private readonly konuApiBase = `${this.apiRoot}/konu`;
  private readonly kategoriApiBase = `${this.apiRoot}/kategori`;

  aktifSekme: 'liste' | 'ekle' = 'ekle';

  tumKonular: KonuKaydi[] = [];
  filtrelenmis: KonuKaydi[] = [];
  aramaMetni = '';
  turFiltresi = '';
  yukleniyor = false;

  secilenKonu: KonuKaydi | null = null;
  altKonular: KonuKaydi[] = [];

  hataMesaji = '';
  basariMesaji = '';

  silModalAcik = false;
  silinecek: KonuKaydi | null = null;
  siliyor = false;

  // Form
  formAd = '';
  formTur: string[] = ['Ana Konu'];
  formAciklama = '';
  formUstKonuId: string | null = null;
  ekleniyor = false;
  kategoriler: KategoriKaydi[] = [];

  readonly varsayilanTurler = [
    'Ana Konu',
    'Ekonomik ve Sosyal Haklar',
    'Isci Haklari Eylemleri',
    'Emekli Haklari Eylemleri',
    'Tarim ve Gida',
    'Cevre ve Iklim',
    'Egitim',
    'Saglik',
    'Konut ve Kentsel Donusum',
    'Siyasi Talepler',
    'Kamu Guvenligi',
    'Diger'
  ];

  readonly turRenkleri: Record<string, { bg: string; color: string; border: string }> = {
    'Ana Konu':                      { bg: '#eaf2ff', color: '#1a5276', border: '#a9cce3' },
    'Ekonomik ve Sosyal Haklar':     { bg: '#fef9e7', color: '#b7950b', border: '#f9e79f' },
    'Isci Haklari Eylemleri':        { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' },
    'Emekli Haklari Eylemleri':      { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' },
    'Tarim ve Gida':                 { bg: '#eafaf1', color: '#1e8449', border: '#a9dfbf' },
    'Cevre ve Iklim':                { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' },
    'Egitim':                        { bg: '#fdecea', color: '#c0392b', border: '#f1948a' },
    'Saglik':                        { bg: '#fdf2e9', color: '#ca6f1e', border: '#fad7a0' },
    'Konut ve Kentsel Donusum':      { bg: '#f0f3f4', color: '#5d6d7e', border: '#d5d8dc' },
    'Siyasi Talepler':               { bg: '#fdedec', color: '#922b21', border: '#f5b7b1' },
    'Kamu Guvenligi':                { bg: '#eaf0fb', color: '#1a5276', border: '#85c1e9' },
    'Diger':                         { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' },
  };

  private kategoriRetryCount = 0;
  private konuRetryCount = 0;
  private readonly maxRetry = 3;

  constructor(private http: HttpClient, private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.kategorileriGetir();
    this.konulariGetir();
  }

  kategorileriGetir(tercihEdilenId?: string | null, tercihEdilenAd?: string): void {
    this.http.get<KategoriKaydi[]>(this.kategoriApiBase).subscribe({
      next: (data) => {
        this.kategoriRetryCount = 0;
        this.kategoriler = this.kategorileriSirala(data.filter(k => !!k.ad?.trim()));

        this.kategoriSeciminiEsitle(tercihEdilenId, tercihEdilenAd);
        this.cdr.markForCheck();
      },
      error: (err) => {
        if (this.kategoriRetryCount < this.maxRetry) {
          this.kategoriRetryCount++;
          timer(1000 * this.kategoriRetryCount).pipe(takeUntil(this.destroy$)).subscribe(() => this.kategorileriGetir(tercihEdilenId, tercihEdilenAd));
        } else {
          this.hataMesaji = this.hataMetni(err, 'Kategoriler yuklenemedi.');
          this.kategoriRetryCount = 0;
          this.cdr.markForCheck();
        }
      },
    });
  }

  private kategoriSeciminiEsitle(tercihEdilenId?: string | null, tercihEdilenAd?: string): void {
    console.log('Tercih Edilen ID:', tercihEdilenId);
    console.log('Tercih Edilen Ad:', tercihEdilenAd);
    const secilecek = this.kategoriler.find(k => k.id === tercihEdilenId) ??
      this.kategoriler.find(k => (k.ad ?? '') === tercihEdilenAd) ??
      this.kategoriler[0] ??
      null;

    this.formTur = secilecek?.ad ? [secilecek.ad] : [];
    console.log('Form Tür:', this.formTur);
    this.seciliKategoriId = secilecek?.id ?? null;
  }

  private kategorileriSirala(kategoriler: KategoriKaydi[]): KategoriKaydi[] {
    return [...kategoriler].sort((sol, sag) => {
      const solAd = sol.ad ?? '';
      const sagAd = sag.ad ?? '';
      const solIndex = this.varsayilanTurler.indexOf(solAd);
      const sagIndex = this.varsayilanTurler.indexOf(sagAd);

      if (solIndex !== -1 && sagIndex !== -1) {
        return solIndex - sagIndex;
      }

      if (solIndex !== -1) {
        return -1;
      }

      if (sagIndex !== -1) {
        return 1;
      }

      return solAd.localeCompare(sagAd, 'tr');
    });
  }

  konulariGetir(): void {
    this.yukleniyor = true;
    this.hataMesaji = '';
    this.http.get<KonuKaydi[]>(this.konuApiBase).subscribe({
      next: (data) => {
        this.konuRetryCount = 0;
        this.tumKonular = data;
        this.filtrele();
        this.yukleniyor = false;
        if (this.secilenKonu) {
          const guncellenmis = data.find(k => k.id === this.secilenKonu!.id);
          if (guncellenmis) this.konuSecHazirla(guncellenmis);
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        if (this.konuRetryCount < this.maxRetry) {
          this.konuRetryCount++;
          timer(1000 * this.konuRetryCount).pipe(takeUntil(this.destroy$)).subscribe(() => this.konulariGetir());
        } else {
          this.hataMesaji = this.hataMetni(err, 'Konular yuklenemedi.');
          this.yukleniyor = false;
          this.konuRetryCount = 0;
          this.cdr.markForCheck();
        }
      },
    });
  }

  filtrele(): void {
    let liste = [...this.tumKonular];
    const ara = this.aramaMetni.toLowerCase().trim();
    if (ara) {
      liste = liste.filter(k =>
        k.ad.toLowerCase().includes(ara) ||
        (k.aciklama ?? '').toLowerCase().includes(ara)
      );
    }
    if (this.turFiltresi) {
      liste = liste.filter(k => k.tur === this.turFiltresi);
    }
    this.filtrelenmis = liste;
  }

  get anaKonular(): KonuKaydi[] {
    return this.tumKonular.filter(k => !k.ustKonuId);
  }

  altKonulariGetir(ustId: string): KonuKaydi[] {
    return this.tumKonular.filter(k => k.ustKonuId === ustId);
  }

  konuSec(k: KonuKaydi): void {
    this.konuSecHazirla(k);
  }

  private konuSecHazirla(k: KonuKaydi): void {
    this.secilenKonu = k;
    this.altKonular = this.altKonulariGetir(k.id);
  }

  panelKapat(): void {
    this.secilenKonu = null;
    this.altKonular = [];
  }

  navigateToUst(id: string): void {
    const ust = this.tumKonular.find(k => k.id === id);
    if (ust) this.konuSecHazirla(ust);
  }

  formGecerliMi(): boolean {
    return !!this.formAd.trim() && !!this.seciliTur;
  }

  konuEkle(): void {
    if (!this.formGecerliMi()) return;
    this.ekleniyor = true;
    this.hataMesaji = '';
    this.basariMesaji = '';
    const body = {
      ad: this.formAd.trim(),
      tur: this.seciliTur,
      aciklama: this.formAciklama.trim() || null,
      ustKonuId: this.formUstKonuId || null,
    };
    this.http.post<KonuKaydi>(this.konuApiBase, body).subscribe({
      next: () => {
        this.ekleniyor = false;
        this.router.navigate(['/konular']);
      },
      error: (err) => {
        this.hataMesaji = this.hataMetni(err, 'Konu eklenemedi.');
        this.ekleniyor = false;
        this.cdr.markForCheck();
      },
    });
  }

  private formuSifirla(): void {
    this.formAd = '';
    this.kategoriSeciminiEsitle(undefined, 'Ana Konu');
    this.formAciklama = '';
    this.formUstKonuId = null;
  }

  silModalAc(k: KonuKaydi, event: Event): void {
    event.stopPropagation();
    this.silinecek = k;
    this.silModalAcik = true;
  }

  konuSil(): void {
    if (!this.silinecek) return;
    this.siliyor = true;
    this.http.delete(`${this.konuApiBase}/${this.silinecek.id}`).subscribe({
      next: () => {
        this.basariMesaji = 'Konu silindi.';
        if (this.secilenKonu?.id === this.silinecek!.id) this.panelKapat();
        this.siliyor = false;
        this.silModalAcik = false;
        this.silinecek = null;
        this.konulariGetir();
      },
      error: (err) => {
        this.hataMesaji = this.hataMetni(err, 'Konu silinemedi.');
        this.siliyor = false;
        this.silModalAcik = false;
        this.cdr.markForCheck();
      },
    });
  }

  modalKapat(): void {
    this.silModalAcik = false;
    this.silinecek = null;
  }

  alertKapat(): void {
    this.hataMesaji = '';
    this.basariMesaji = '';
  }

  turRenk(tur: string | null) {
    return this.turRenkleri[tur ?? 'Diger'] ?? this.turRenkleri['Diger'];
  }

  turSayisi(tur: string): number {
    return this.tumKonular.filter(k => k.tur === tur).length;
  }

  get toplamAltKonuSayisi(): number {
    return this.tumKonular.filter(k => k.ustKonuId).length;
  }

  tarihFormat(tarih: string): string {
    if (!tarih) return '-';
    try { return new Date(tarih).toLocaleDateString('tr-TR'); } catch { return tarih; }
  }

  seciliKategoriId: string | null = null;
  kategoriGirisiAcik = false;
  yeniKategori = '';
  kategoriDuzenlemeAcik = false;
  duzenlenenKategoriId: string | null = null;
  duzenlenenKategoriAdi = '';

  get seciliTur(): string {
    return this.formTur[0] ?? '';
  }

  get turler(): string[] {
    return this.kategoriler.map(k => k.ad ?? '').filter(Boolean);
  }

  kategoriSec(tur: string): void {
    const kategori = this.kategoriler.find(k => k.ad === tur);
    if (!kategori) {
      return;
    }

    this.formTur = [tur];
    this.seciliKategoriId = kategori.id;
  }

  kategoriDuzenlenebilir(tur: string): boolean {
    return !!this.kategoriler.find(k => k.ad === tur);
  }

  kategoriSil(tur: string, event: Event): void {
    event.stopPropagation();

    const kategori = this.kategoriler.find(k => k.ad === tur);
    if (!kategori) {
      return;
    }

    this.http.delete(`${this.kategoriApiBase}/${kategori.id}`).subscribe({
      next: () => {
        const sonrakiId = this.seciliKategoriId === kategori.id
          ? this.kategoriler.find(item => item.id !== kategori.id)?.id ?? null
          : this.seciliKategoriId;

        this.kategorileriGetir(sonrakiId);
        if (this.duzenlenenKategoriId === kategori.id) {
          this.kategoriDuzenlemeyiKapat();
        }
      },
      error: (err) => {
        this.hataMesaji = this.hataMetni(err, 'Kategori silinemedi.');
        this.cdr.markForCheck();
      },
    });
  }

  kategoriGirisiAc(): void {
    this.kategoriGirisiAcik = true;
    this.kategoriDuzenlemeyiKapat();
    this.yeniKategori = '';
  }

  kategoriGirisiKapat(): void {
    this.kategoriGirisiAcik = false;
    this.yeniKategori = '';
  }

  kategoriEkle(): void {
    const yeniDeger = this.yeniKategori.trim();
    if (!yeniDeger) return;

    const mevcut = this.kategoriler.find(t => (t.ad ?? '').toLocaleLowerCase('tr-TR') === yeniDeger.toLocaleLowerCase('tr-TR'));
    if (mevcut) {
      this.kategoriSec(mevcut.ad ?? '');
      this.kategoriGirisiKapat();
      return;
    }

    this.http.post<KategoriKaydi>(this.kategoriApiBase, JSON.stringify(yeniDeger), {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }).subscribe({
      next: (kategori) => {
        this.turRenkleri[yeniDeger] = this.turRenkleri['Diger'];
        this.kategoriGirisiKapat();
        this.kategorileriGetir(kategori.id, kategori.ad ?? yeniDeger);
      },
      error: (err) => {
        this.hataMesaji = this.hataMetni(err, 'Kategori eklenemedi.');
        this.cdr.markForCheck();
      },
    });
  }

  kategoriDuzenlemeAc(tur: string, event: Event): void {
    event.stopPropagation();

    const kategori = this.kategoriler.find(k => k.ad === tur);
    if (!kategori) {
      return;
    }

    this.kategoriGirisiAcik = false;
    this.kategoriDuzenlemeAcik = true;
    this.duzenlenenKategoriId = kategori.id;
    this.duzenlenenKategoriAdi = kategori.ad ?? '';
  }

  kategoriDuzenlemeyiKapat(): void {
    this.kategoriDuzenlemeAcik = false;
    this.duzenlenenKategoriId = null;
    this.duzenlenenKategoriAdi = '';
  }

  kategoriGuncelle(): void {
    const yeniAd = this.duzenlenenKategoriAdi.trim();
    if (!this.duzenlenenKategoriId || !yeniAd) {
      return;
    }

    const ayniIsimleBaskaKategoriVar = this.kategoriler.some(k =>
      k.id !== this.duzenlenenKategoriId &&
      (k.ad ?? '').toLocaleLowerCase('tr-TR') === yeniAd.toLocaleLowerCase('tr-TR')
    );

    if (ayniIsimleBaskaKategoriVar) {
      this.hataMesaji = 'Ayni isimde baska bir kategori zaten mevcut.';
      return;
    }

    this.http.put(`${this.kategoriApiBase}/${this.duzenlenenKategoriId}`, JSON.stringify(yeniAd), {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' })
    }).subscribe({
      next: () => {
        this.kategoriDuzenlemeyiKapat();
        this.kategorileriGetir(this.duzenlenenKategoriId, yeniAd);
      },
      error: (err) => {
        this.hataMesaji = this.hataMetni(err, 'Kategori guncellenemedi.');
        this.cdr.markForCheck();
      },
    });
  }

  private hataMetni(err: unknown, varsayilan: string): string {
    if (!(err instanceof HttpErrorResponse)) {
      return varsayilan;
    }

    const payload = err.error;

    if (typeof payload === 'string' && payload.trim()) {
      return payload;
    }

    if (payload && typeof payload === 'object') {
      const obj = payload as Record<string, unknown>;
      if (typeof obj['message'] === 'string' && obj['message'].trim()) {
        return obj['message'];
      }

      if (typeof obj['title'] === 'string' && obj['title'].trim()) {
        return obj['title'];
      }

      if (Array.isArray(obj['errors'])) {
        const ilk = obj['errors'].find(item => typeof item === 'string' && item.trim());
        if (typeof ilk === 'string') {
          return ilk;
        }
      }

      if (obj['errors'] && typeof obj['errors'] === 'object') {
        const detay = Object.values(obj['errors'] as Record<string, unknown>)
          .flatMap(v => Array.isArray(v) ? v : [v])
          .find(v => typeof v === 'string' && v.trim());
        if (typeof detay === 'string') {
          return detay;
        }
      }
    }

    if (typeof err.message === 'string' && err.message.trim()) {
      return err.message;
    }

    return varsayilan;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}