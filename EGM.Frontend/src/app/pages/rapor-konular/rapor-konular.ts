import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ActivatedRoute, Router } from '@angular/router';

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

@Component({
  selector: 'app-rapor-konular',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './rapor-konular.html',
  styleUrls: ['./rapor-konular.css'],
})
export class RaporKonular implements OnInit {
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
    ustKonuId: string | null = null;
  tumKayitlar: KonuKaydi[] = [];
  filtrelenmis: KonuKaydi[] = [];

  aramaMetni = '';
  turFiltresi = '';
  yukleniyor = false;
  hataMesaji = '';

  get turler(): string[] {
    return [...new Set(this.tumKayitlar
      .filter(k => this.ustKonuId ? k.ustKonuId === this.ustKonuId : false)
      .map(k => k.tur ?? '').filter(Boolean))];
  }

  constructor(private http: HttpClient, private route: ActivatedRoute, public router: Router) {}

  konuDetayaGit(id: string): void {
    this.router.navigate(['/konu-detay', id]);
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.ustKonuId = params['ustKonuId'] || null;
      this.verileriYukle();
    });
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || '';
    return new HttpHeaders({ Authorization: `Bearer ${token}` });
  }

  verileriYukle(): void {
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
      const altKonuUygun = this.ustKonuId ? k.ustKonuId === this.ustKonuId : false;
      return metinUygun && turUygun && altKonuUygun;
    });
  }

  get ustKonuAd(): string {
    return this.filtrelenmis[0]?.ustKonuAd ?? '';
  }

  trackById(_index: number, item: KonuKaydi): string {
    return item.id;
  }
}
