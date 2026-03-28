import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

const API = environment.apiUrl;

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

interface KategoriKaydi {
  id: string;
  ad: string | null;
}

@Component({
  selector: 'app-konular',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './konular.html',
  styleUrls: ['./konular.css'],
})
export class Konular implements OnInit {
    altKonularaGit(id: string): void {
      this.router.navigate(['/rapor-konular'], { queryParams: { ustKonuId: id } });
    }
  tumKayitlar: KonuKaydi[] = [];
  filtrelenmis: KonuKaydi[] = [];
  turler: string[] = [];

  aramaMetni = '';
  turFiltresi = '';
  yukleniyor = false;
  hataMesaji = '';

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

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    this.kategorileriYukle();
    this.konulariYukle();
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  kategorileriYukle(): void {
    this.http.get<KategoriKaydi[]>(`${API}/api/organizator/kategori`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => {
          this.turler = data.map(k => k.ad ?? '').filter(Boolean);
        },
        error: () => {}
      });
  }

  konulariYukle(): void {
    this.yukleniyor = true;
    this.hataMesaji = '';
    this.http.get<KonuKaydi[]>(`${API}/api/organizator/konu`, { headers: this.getHeaders() })
      .subscribe({
        next: (data) => {
          this.tumKayitlar = data;
          this.filtrele();
          this.yukleniyor = false;
        },
        error: () => {
          this.hataMesaji = 'Konu verileri yüklenemedi. Bağlantıyı kontrol edin.';
          this.yukleniyor = false;
        }
      });
  }

  filtrele(): void {
    const ara = this.aramaMetni.toLowerCase().trim();
    this.filtrelenmis = this.tumKayitlar.filter(k => {
      const metinUygun = !ara ||
        k.ad.toLowerCase().includes(ara) ||
        (k.aciklama ?? '').toLowerCase().includes(ara);
      const turUygun = !this.turFiltresi || k.tur === this.turFiltresi;
      return metinUygun && turUygun;
    });
  }

  turRenk(tur: string | null) {
    return this.turRenkleri[tur ?? 'Diger'] ?? this.turRenkleri['Diger'];
  }

  get anaKonuSayisi(): number {
    return this.tumKayitlar.filter(k => !k.ustKonuId).length;
  }

  get altKonuSayisi(): number {
    return this.tumKayitlar.filter(k => !!k.ustKonuId).length;
  }

  get turIstatistikleri(): { tur: string; sayi: number }[] {
    const map = new Map<string, number>();
    for (const k of this.tumKayitlar) {
      const tur = k.tur ?? 'Belirtilmemiş';
      map.set(tur, (map.get(tur) ?? 0) + 1);
    }
    return Array.from(map.entries())
      .map(([tur, sayi]) => ({ tur, sayi }))
      .sort((a, b) => b.sayi - a.sayi);
  }

  konuIslemlereGit(): void {
    this.router.navigate(['/konu-islemleri']);
  }
}
