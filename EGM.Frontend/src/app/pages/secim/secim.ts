import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { catchError, of } from 'rxjs';
import { OlayTuruService, OlayTuru } from '../../services/olay-turu.service';
import { GerceklesmeSekliService, GerceklesmeSekli } from '../../services/gerceklesme-sekli.service';
import { KonuService, Konu } from '../../services/konu.service';

const API = 'http://localhost:5117/api';

interface SandikOlayRecord {
  id: string;
  musahitAdi: string;
  il: string;
  ilce: string;
  mahalle: string;
  okul: string;
  sandikNo: number;
  olayKategorisi: string;
  olaySaati: string;
  aciklama: string;
  kanitDosyasi: string | null;
  tarih: string;
  createdAt: string;
  takipNo?: string | null;
}

@Component({
  selector: 'app-secim',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './secim.html',
  styleUrls: ['./secim.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Secim implements OnInit {
  private apiBase = API;
  tumIller: { name: string; osmId: number }[] = [];

  // Form alanlar (ASCII guvenli property isimleri)
  musahitAdi = '';
  il = '';
  ilce = '';
  mahalle = '';
  okul = '';
  konu = '';
  sandikNo: number | null = null;
  olayKategorisi = '';
  olayTarihi = new Date().toISOString().slice(0, 16);
  aciklama = '';
  kanitDosyasiBase64: string | null = null;
  kanitDosyasiAd = '';
  katilimciSayisi: number | null = null;
  sehitSayisi: number | null = null;
  oluSayisi: number | null = null;
  gozaltiSayisi: number | null = null;

  // Konu listesi
  konular: Konu[] = [];
  konularYukleniyor = false;

  // Olay Turu / Gerceklesme Sekli
  olayTurleri: OlayTuru[] = [];
  gerceklesmeSekilleri: GerceklesmeSekli[] = [];
  filtreliGerceklesmeSekilleri: GerceklesmeSekli[] = [];
  secilenOlayTuruId = '';
  secilenOlayTuruAdi = '';
  secilenGerceklesmeSekliId = '';
  secilenGerceklesmeSekliAdi = '';
  turlerYukleniyor = true;
  turlerHata = false;

  // UI durumu
  kayitlar: SandikOlayRecord[] = [];
  gonderiliyor = false;
  hata = '';
  basari = '';
  takipNo: string | null = null;
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

  filtreliIlceler: { name: string; osmId: number }[] = [];
  filtreliMahalleler: { name: string; osmId: number }[] = [];



  get iller(): string[] {
    return this.tumIller.map(i => i.name);
  }

  constructor(private http: HttpClient,
              private olayTuruService: OlayTuruService,
              private gerceklesmeSekliService: GerceklesmeSekliService,
              private konuService: KonuService,
              private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.kayitlariYukle();
    this.konuService.getAll().subscribe({
      next: res => { this.konular = res; this.cdr.markForCheck(); },
      error: () => {}
    });
    this.olayTuruService.getAll().subscribe({
      next: res => { this.olayTurleri = res; this.turlerYukleniyor = false; this.cdr.markForCheck(); },
      error: () => { this.turlerYukleniyor = false; this.turlerHata = true; this.cdr.markForCheck(); }
    });
    this.gerceklesmeSekliService.getAll().subscribe({
      next: res => { this.gerceklesmeSekilleri = res; this.cdr.markForCheck(); },
      error: () => {}
    });
    this.http.get<any[]>(`${API}/geo/provinces-geopackage`).subscribe({
      next: res => { this.tumIller = res; this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  kayitlariYukle(): void {
    this.yukleniyor = true;
    this.http.get<SandikOlayRecord[]>(`${this.apiBase}/secim/sandik-olay`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => { this.kayitlar = data; this.yukleniyor = false; this.cdr.markForCheck(); },
        error: () => { this.hata = 'Kayitlar yuklenemedi.'; this.yukleniyor = false; this.cdr.markForCheck(); }
      });
  }

  ilDegisti(ilAdi: string): void {
    this.filtreliIlceler = [];
    this.filtreliMahalleler = [];
    this.ilce = '';
    this.mahalle = '';
    if (!ilAdi) { this.cdr.markForCheck(); return; }
    this.http.get<any[]>(`${API}/geo/districts-geopackage?province=${encodeURIComponent(ilAdi)}`)
      .pipe(catchError(() => of([])))
      .subscribe(districts => { this.filtreliIlceler = districts; this.cdr.markForCheck(); });
  }

  ilceDegisti(ilceAdi: string): void {
    this.filtreliMahalleler = [];
    this.mahalle = '';
    if (!ilceAdi) { this.cdr.markForCheck(); return; }
    this.http.get<any[]>(`${API}/geo/neighborhoods-geopackage?district=${encodeURIComponent(ilceAdi)}`)
      .pipe(catchError(() => of([])))
      .subscribe(neighborhoods => { this.filtreliMahalleler = neighborhoods; this.cdr.markForCheck(); });
  }

  mahalleDegisti(): void {
    // Placeholder for future coordinate auto-fill
  }

  kategoriSec(k: string): void {
    this.olayKategorisi = this.olayKategorisi === k ? '' : k;
  }

  olayTuruIdDegisti(id: string): void {
    const tur = this.olayTurleri.find(t => t.id === id);
    this.secilenOlayTuruAdi = tur?.name ?? '';
    this.secilenGerceklesmeSekliId = '';
    this.secilenGerceklesmeSekliAdi = '';
    this.filtreliGerceklesmeSekilleri = id
      ? this.gerceklesmeSekilleri.filter(s => s.olayTuruId === id)
      : [];
    this.olayKategorisi = this.secilenOlayTuruAdi;
  }

  gerceklesmeSekliIdDegisti(id: string): void {
    const sekli = this.filtreliGerceklesmeSekilleri.find(s => s.id === id);
    this.secilenGerceklesmeSekliAdi = sekli?.name ?? '';
    this.olayKategorisi = this.secilenGerceklesmeSekliAdi
      ? `${this.secilenOlayTuruAdi} / ${this.secilenGerceklesmeSekliAdi}`
      : this.secilenOlayTuruAdi;
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
      !!this.il && !!this.ilce.trim() && !!this.secilenOlayTuruId && !!this.secilenGerceklesmeSekliId &&
      !!this.olayTarihi && this.aciklama.trim().length > 0 &&
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
      konu: this.konu || null,
      olayKategorisi: this.olayKategorisi,
      olaySaati: this.olayTarihi.slice(11) + ':00',
      okul: this.okul.trim() || null,
      aciklama: this.aciklama.trim(),
      kanitDosyasi: this.kanitDosyasiBase64,
      tarih: new Date(this.olayTarihi).toISOString(),
      katilimciSayisi: this.katilimciSayisi ?? 0,
      sehitSayisi: this.sehitSayisi ?? 0,
      oluSayisi: this.oluSayisi ?? 0,
      gozaltiSayisi: this.gozaltiSayisi ?? 0
    };

    this.http.post<SandikOlayRecord>(`${this.apiBase}/secim/sandik-olay`, payload, { headers: this.getHeaders() })
      .subscribe({
        next: (yeni) => {
          this.kayitlar.unshift(yeni);
          this.takipNo = yeni.takipNo ?? null;
          this.formuSifirla();
          this.basari = 'Sandik olayi basariyla kaydedildi.';
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
          this.cdr.markForCheck();
        },
        error: () => { this.hata = 'Silme islemi basarisiz oldu.'; this.cdr.markForCheck(); }
      });
  }

  private formuSifirla(): void {
    this.musahitAdi = '';
    this.il = '';
    this.ilce = '';
    this.mahalle = '';
    this.sandikNo = null;
    this.olayKategorisi = '';
    this.secilenOlayTuruId = '';
    this.secilenOlayTuruAdi = '';
    this.secilenGerceklesmeSekliId = '';
    this.secilenGerceklesmeSekliAdi = '';
    this.filtreliGerceklesmeSekilleri = [];
    this.okul = '';
    this.konu = '';
    this.olayTarihi = new Date().toISOString().slice(0, 16);
    this.aciklama = '';
    this.kanitDosyasiBase64 = null;
    this.kanitDosyasiAd = '';
  }

  tarihFormat(dateStr: string): string {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleDateString('tr-TR');
  }

  kategoriSayisi(k: string): number {
    return this.kayitlar.filter(r => r.olayKategorisi?.startsWith(k)).length;
  }

  kategoriRenk(k: string): string {
    const renkler: Record<string, string> = {
      'Musahit Engellenmesi': '#e74c3c',
      'Mukerrer Oy Denemesi': '#e67e22',
      'Kavga / Ariza': '#c0392b',
      'Sayim Hatasi': '#8e44ad',
      'Diger': '#7f8c8d'
    };
    if (renkler[k]) return renkler[k];
    const palette = ['#2980b9','#27ae60','#8e44ad','#e74c3c','#e67e22','#16a085','#d35400','#2c3e50'];
    let hash = 0;
    for (let i = 0; i < k.length; i++) hash = k.charCodeAt(i) + ((hash << 5) - hash);
    return palette[Math.abs(hash) % palette.length];
  }
}