import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface SosyalMedyaForm {
  kullaniciAdi: string;
  paylasimLinki: string;
  platform: string;
  paylasimTarihi: string;
  icerikOzeti: string;
  hassasiyet: number;
  sosyalSignalSkoru: number;
  ekranGoruntusuBase64: string;
  ekranGoruntusuAd: string;
}

@Component({
  selector: 'app-socialmedia',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './socialmedia.html',
  styleUrls: ['./socialmedia.css'],
})
export class Socialmedia {
  private apiBase = 'http://localhost:5117/api';

  kaydediliyor = false;
  basariMesaji = '';
  hataMesaji = '';

  form: SosyalMedyaForm = {
    kullaniciAdi: '',
    paylasimLinki: '',
    platform: '',
    paylasimTarihi: new Date().toISOString().slice(0, 16),
    icerikOzeti: '',
    hassasiyet: 0,
    sosyalSignalSkoru: 0,
    ekranGoruntusuBase64: '',
    ekranGoruntusuAd: '',
  };

  readonly platformlar = [
    'Twitter / X',
    'Facebook',
    'Instagram',
    'YouTube',
    'TikTok',
    'Telegram',
    'WhatsApp',
    'LinkedIn',
    'Diger',
  ];

  constructor(private http: HttpClient) {}

  gorselSec(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const file = input.files[0];
    if (!file.type.startsWith('image/')) {
      this.hataMesaji = 'Lutfen gecerli bir gorsel dosyasi secin.';
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.hataMesaji = 'Gorsel boyutu 5 MB den buyuk olamaz.';
      return;
    }
    this.hataMesaji = '';
    this.form.ekranGoruntusuAd = file.name;
    const reader = new FileReader();
    reader.onload = (e) => {
      this.form.ekranGoruntusuBase64 = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  gorselKaldir(): void {
    this.form.ekranGoruntusuBase64 = '';
    this.form.ekranGoruntusuAd = '';
  }

  kaydet(): void {
    if (!this.form.kullaniciAdi.trim()) {
      this.hataMesaji = 'Kullanici adi zorunludur.';
      return;
    }
    if (!this.form.paylasimLinki.trim()) {
      this.hataMesaji = 'Paylasim linki zorunludur.';
      return;
    }
    if (!this.form.platform) {
      this.hataMesaji = 'Platform secimi zorunludur.';
      return;
    }
    if (!this.form.icerikOzeti.trim()) {
      this.hataMesaji = 'Paylasim aciklamasi zorunludur.';
      return;
    }

    this.kaydediliyor = true;
    this.hataMesaji = '';
    this.basariMesaji = '';

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    const payload = {
      olayId: null,
      platform: this.form.platform,
      paylasimLinki: this.form.paylasimLinki,
      paylasimTarihi: this.form.paylasimTarihi,
      icerikOzeti: this.form.icerikOzeti,
      ilgiliKisiKurum: this.form.kullaniciAdi,
      ekranGoruntusu: this.form.ekranGoruntusuBase64 || null,
      hassasiyet: this.form.hassasiyet,
      sosyalSignalSkoru: this.form.sosyalSignalSkoru,
    };

    this.http
      .post(`${this.apiBase}/sosyalmedyaolay`, payload, { headers })
      .subscribe({
        next: () => {
          this.basariMesaji = 'Sosyal medya olayi basariyla kaydedildi.';
          this.kaydediliyor = false;
          this.formSifirla();
        },
        error: (err) => {
          this.hataMesaji = err?.error?.title || 'Kayit sirasinda hata olustu.';
          this.kaydediliyor = false;
        },
      });
  }

  private formSifirla(): void {
    this.form = {
      kullaniciAdi: '',
      paylasimLinki: '',
      platform: '',
      paylasimTarihi: new Date().toISOString().slice(0, 16),
      icerikOzeti: '',
      hassasiyet: 0,
      sosyalSignalSkoru: 0,
      ekranGoruntusuBase64: '',
      ekranGoruntusuAd: '',
    };
  }
}