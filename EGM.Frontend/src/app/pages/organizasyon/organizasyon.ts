import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

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
}

@Component({
  selector: 'app-organizasyon',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './organizasyon.html',
  styleUrls: ['./organizasyon.css'],
})
export class Organizasyon implements OnInit {
  private readonly apiBase = 'http://localhost:5117/api/organizator';

  aktifSekme: 'liste' | 'ekle' = 'liste';

  tumKuruluslar: KurulusKaydi[] = [];
  filtrelenmis: KurulusKaydi[] = [];
  aramaMetni = '';
  turFiltresi = '';
  yukleniyor = false;

  // Detay paneli
  secilenKurulus: KurulusKaydi | null = null;
  altKuruluslar: KurulusKaydi[] = [];

  hataMesaji = '';
  basariMesaji = '';

  // Silme modal
  silModalAcik = false;
  silinecek: KurulusKaydi | null = null;
  siliyor = false;

  // Form
  formAd = '';
  formTur = 'Sendika';
  formFaaliyetAlani = '';
  formIletisim = '';
  formAciklama = '';
  formKurulusTarihi = '';
  formUstKurulusId: string | null = null;
  ekleniyor = false;

  readonly turler = [
    'Konfederasyon', 'Sendika', 'Siyasi Parti', 'Dernek', 'Vakif', 'STK', 'Kamu Kurumu', 'Diger'
  ];

  readonly turRenkleri: Record<string, { bg: string; color: string; border: string }> = {
    'Konfederasyon':  { bg: '#fef9e7', color: '#d68910', border: '#f9e79f' },
    'Sendika':        { bg: '#ebf5fb', color: '#2980b9', border: '#aed6f1' },
    'Siyasi Parti':   { bg: '#fdecea', color: '#c0392b', border: '#f1948a' },
    'Dernek':         { bg: '#e8f8f5', color: '#16a085', border: '#a2d9ce' },
    'Vakif':          { bg: '#f5eef8', color: '#8e44ad', border: '#d2b4de' },
    'STK':            { bg: '#eafaf1', color: '#1e8449', border: '#a9dfbf' },
    'Kamu Kurumu':    { bg: '#eaf2ff', color: '#1a5276', border: '#a9cce3' },
    'Diger':          { bg: '#f0f3f4', color: '#7f8c8d', border: '#d5d8dc' },
  };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.kuruluslariGetir();
  }

  kuruluslariGetir(): void {
    this.yukleniyor = true;
    this.hataMesaji = '';
    this.http.get<KurulusKaydi[]>(this.apiBase).subscribe({
      next: (data) => {
        this.tumKuruluslar = data;
        this.filtrele();
        this.yukleniyor = false;
        // Acik detay varsa guncelle
        if (this.secilenKurulus) {
          const guncellenmis = data.find(k => k.id === this.secilenKurulus!.id);
          if (guncellenmis) this.kurulusSecHazirla(guncellenmis);
        }
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Kuruluslar yuklenemedi.';
        this.yukleniyor = false;
      },
    });
  }

  filtrele(): void {
    let liste = [...this.tumKuruluslar];
    const ara = this.aramaMetni.toLowerCase().trim();
    if (ara) {
      liste = liste.filter(k =>
        k.ad.toLowerCase().includes(ara) ||
        (k.faaliyetAlani ?? '').toLowerCase().includes(ara) ||
        (k.aciklama ?? '').toLowerCase().includes(ara)
      );
    }
    if (this.turFiltresi) {
      liste = liste.filter(k => k.tur === this.turFiltresi);
    }
    this.filtrelenmis = liste;
  }

  // Sadece ust duzey (bagimsiz) kuruluslar
  get ustDuzeyKuruluslar(): KurulusKaydi[] {
    return this.tumKuruluslar.filter(k => !k.ustKurulusId);
  }

  // Alt duzey kuruluslar (belli bir uste bagli)
  altKuruluslariGetir(ustId: string): KurulusKaydi[] {
    return this.tumKuruluslar.filter(k => k.ustKurulusId === ustId);
  }

  kurulusSec(k: KurulusKaydi): void {
    this.kurulusSecHazirla(k);
  }

  private kurulusSecHazirla(k: KurulusKaydi): void {
    this.secilenKurulus = k;
    this.altKuruluslar = this.altKuruluslariGetir(k.id);
  }

  panelKapat(): void {
    this.secilenKurulus = null;
    this.altKuruluslar = [];
  }

  // ── Ekleme ─────────────────────────────────────────────────────────────
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
    };
    this.http.post<KurulusKaydi>(this.apiBase, body).subscribe({
      next: (yeni) => {
        this.basariMesaji = `"${yeni.ad}" basariyla eklendi.`;
        this.ekleniyor = false;
        this.formuSifirla();
        this.aktifSekme = 'liste';
        this.kuruluslariGetir();
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Kurulus eklenemedi.';
        this.ekleniyor = false;
      },
    });
  }

  private formuSifirla(): void {
    this.formAd = ''; this.formTur = 'Sendika';
    this.formFaaliyetAlani = ''; this.formIletisim = '';
    this.formAciklama = ''; this.formKurulusTarihi = '';
    this.formUstKurulusId = null;
  }

  // ── Silme ──────────────────────────────────────────────────────────────
  silModalAc(k: KurulusKaydi, event: Event): void {
    event.stopPropagation();
    this.silinecek = k;
    this.silModalAcik = true;
  }

  kurulusSil(): void {
    if (!this.silinecek) return;
    this.siliyor = true;
    this.http.delete(`${this.apiBase}/${this.silinecek.id}`).subscribe({
      next: () => {
        this.basariMesaji = 'Kurulus silindi.';
        if (this.secilenKurulus?.id === this.silinecek!.id) this.panelKapat();
        this.siliyor = false;
        this.silModalAcik = false;
        this.silinecek = null;
        this.kuruluslariGetir();
      },
      error: (err) => {
        this.hataMesaji = err?.error ?? 'Kurulus silinemedi.';
        this.siliyor = false;
        this.silModalAcik = false;
      },
    });
  }

  modalKapat(): void {
    this.silModalAcik = false;
    this.silinecek = null;
  }

  alertKapat(): void { this.hataMesaji = ''; this.basariMesaji = ''; }

  // ── Yardimci ──────────────────────────────────────────────────────────
  turRenk(tur: string) {
    return this.turRenkleri[tur] ?? this.turRenkleri['Diger'];
  }

  tarihFormat(tarih: string): string {
    if (!tarih) return '-';
    try { return new Date(tarih).toLocaleDateString('tr-TR'); } catch { return tarih; }
  }

  turSayisi(tur: string): number {
    return this.tumKuruluslar.filter(k => k.tur === tur).length;
  }

  get toplamAltKurulusSayisi(): number {
    return this.tumKuruluslar.filter(k => k.ustKurulusId).length;
  }

  navigateToUst(id: string): void {
    const ust = this.tumKuruluslar.find(k => k.id === id);
    if (ust) this.kurulusSecHazirla(ust);
  }
}