import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
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
  siyasiYonelim?: string;
  kutukNumarasi?: string;
  telefon?: string;
  eposta?: string;
}

@Component({
  selector: 'app-rapor-kuruluslar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rapor-kuruluslar.html',
  styleUrls: ['./rapor-kuruluslar.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RaporKuruluslar implements OnInit {
  private readonly apiBase = `${environment.apiUrl}/api/organizator`;

  tumKuruluslar: KurulusKaydi[] = [];
  filtrelenmis: KurulusKaydi[] = [];
  aramaMetni = '';
  turFiltresi = '';
  yonelimFiltresi = '';
  yukleniyor = false;

  secilenKurulus: KurulusKaydi | null = null; // silme modal referansı için tutulur

  hataMesaji = '';
  basariMesaji = '';

  silModalAcik = false;
  silinecek: KurulusKaydi | null = null;
  siliyor = false;

  orgLogolari: Record<string, string> = {};

  readonly turler = [
    'Konfederasyon', 'Sendika', 'Siyasi Parti', 'Dernek', 'Vakif', 'STK', 'Kamu Kurumu', 'Diger',
  ];

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

  readonly durumEtiketleri: Record<number, { ad: string; bg: string; color: string }> = {
    0: { ad: 'Planlandi', bg: '#fff3cd', color: '#856404' },
    1: { ad: 'Devam Ediyor', bg: '#d1ecf1', color: '#0c5460' },
    2: { ad: 'Tamamlandi', bg: '#d4edda', color: '#155724' },
    3: { ad: 'Iptal Edildi', bg: '#f8d7da', color: '#721c24' },
  };

  constructor(private http: HttpClient, private router: Router, private route: ActivatedRoute, private cdr: ChangeDetectorRef) {}

  private acilacakKurulusId: string | null = null;

  ngOnInit(): void {
    this.orgLogolariYukle();
    this.route.queryParamMap.subscribe(params => {
      this.acilacakKurulusId = params.get('id');
      if (params.get('mesaj') === 'kayit') {
        this.basariMesaji = 'Kuruluş kaydedildi.';
      }
    });
    this.kuruluslariGetir();
  }

  private orgLogolariYukle(): void {
    const kayitli = localStorage.getItem('egm_org_logolar');
    if (kayitli) {
      try { this.orgLogolari = JSON.parse(kayitli); } catch { this.orgLogolari = {}; }
    }
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  kuruluslariGetir(): void {
    this.yukleniyor = true;
    this.hataMesaji = '';
    this.http.get<KurulusKaydi[]>(this.apiBase, { headers: this.getHeaders() }).subscribe({
      next: (data) => {
        this.tumKuruluslar = data;
        this.filtrele();
        this.yukleniyor = false;
        this.acilacakKurulusId = null;
        this.cdr.markForCheck();
      },
      error: (err) => {
        const e = err?.error;
        this.hataMesaji = typeof e === 'string' ? e : (e?.title ?? e?.message ?? (`Hata ${err?.status ?? ''}: Kuruluslar yuklenemedi.`).trim());
        this.yukleniyor = false;
        this.cdr.markForCheck();
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
        (k.aciklama ?? '').toLowerCase().includes(ara) ||
        (k.kutukNumarasi ?? '').toLowerCase().includes(ara)
      );
    }
    if (this.turFiltresi) {
      liste = liste.filter(k => k.tur === this.turFiltresi);
    }
    if (this.yonelimFiltresi) {
      liste = liste.filter(k => (k.siyasiYonelim ?? '') === this.yonelimFiltresi);
    }
    this.filtrelenmis = liste;
  }

  get mevcutYonelimler(): string[] {
    const set = new Set<string>();
    this.tumKuruluslar.forEach(k => { if (k.siyasiYonelim) set.add(k.siyasiYonelim); });
    return Array.from(set).sort();
  }

  kurulusSec(k: KurulusKaydi): void {
    this.router.navigate(['/kurulus-detay', k.id]);
  }

  silModalAc(k: KurulusKaydi, event: Event): void {
    event.stopPropagation();
    this.silinecek = k;
    this.silModalAcik = true;
  }

  kurulusSil(): void {
    if (!this.silinecek) return;
    this.siliyor = true;
    this.http.delete(`${this.apiBase}/${this.silinecek.id}`, { headers: this.getHeaders() }).subscribe({
      next: () => {
        this.basariMesaji = 'Kurulus silindi.';
        this.siliyor = false;
        this.silModalAcik = false;
        this.silinecek = null;
        this.kuruluslariGetir();
      },
      error: (err) => {
        const e2 = err?.error;
        this.hataMesaji = typeof e2 === 'string' ? e2 : (e2?.title ?? e2?.message ?? 'Kurulus silinemedi.');
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

  navigateToEkle(): void {
    this.router.navigate(['/organizasyon']);
  }
}

