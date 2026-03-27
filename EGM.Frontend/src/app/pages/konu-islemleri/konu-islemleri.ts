import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

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
})
export class KonuIslemleri implements OnInit {
  private readonly apiBase = 'http://localhost:5117/api/organizator/konu';

  aktifSekme: 'liste' | 'ekle' = 'liste';

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
  formTur = 'Ana Konu';
  formAciklama = '';
  formUstKonuId: string | null = null;
  ekleniyor = false;

  readonly turler = [
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

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.konulariGetir();
  }

  konulariGetir(): void {
    this.yukleniyor = true;
    this.hataMesaji = '';
    this.http.get<KonuKaydi[]>(this.apiBase).subscribe({
      next: (data) => {
        this.tumKonular = data;
        this.filtrele();
        this.yukleniyor = false;
        if (this.secilenKonu) {
          const guncellenmis = data.find(k => k.id === this.secilenKonu!.id);
          if (guncellenmis) this.konuSecHazirla(guncellenmis);
        }
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Konular yuklenemedi.';
        this.yukleniyor = false;
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
    return !!(this.formAd.trim() && this.formTur);
  }

  konuEkle(): void {
    if (!this.formGecerliMi()) return;
    this.ekleniyor = true;
    this.hataMesaji = '';
    this.basariMesaji = '';
    const body = {
      ad: this.formAd.trim(),
      tur: this.formTur,
      aciklama: this.formAciklama.trim() || null,
      ustKonuId: this.formUstKonuId || null,
    };
    this.http.post<KonuKaydi>(this.apiBase, body).subscribe({
      next: (yeni) => {
        this.basariMesaji = `"${yeni.ad}" basariyla eklendi.`;
        this.ekleniyor = false;
        this.formuSifirla();
        this.aktifSekme = 'liste';
        this.konulariGetir();
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Konu eklenemedi.';
        this.ekleniyor = false;
      },
    });
  }

  private formuSifirla(): void {
    this.formAd = '';
    this.formTur = 'Ana Konu';
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
    this.http.delete(`${this.apiBase}/${this.silinecek.id}`).subscribe({
      next: () => {
        this.basariMesaji = 'Konu silindi.';
        if (this.secilenKonu?.id === this.silinecek!.id) this.panelKapat();
        this.siliyor = false;
        this.silModalAcik = false;
        this.silinecek = null;
        this.konulariGetir();
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Konu silinemedi.';
        this.siliyor = false;
        this.silModalAcik = false;
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
}