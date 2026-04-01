import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

interface VeriOzeti {
  tur: string;
  ikon: string;
  renk: string;
  sayi: number;
  route: string;
}

interface SonAktivite {
  tip: string;
  aciklama: string;
  tarih: string;
  kullanici: string;
}

@Component({
  selector: 'app-veri-yonetimi',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './veri-yonetimi.html',
  styleUrls: ['./veri-yonetimi.css'],
})
export class VeriYonetimi implements OnInit {
  yukleniyor = false;
  hata: string | null = null;

  readonly baseUrl = 'http://localhost:5117';

  ozet: VeriOzeti[] = [
    { tur: 'Olay', ikon: 'alert', renk: '#e74c3c', sayi: 0, route: '/olay' },
    { tur: 'Sokak Olayı', ikon: 'walk', renk: '#e67e22', sayi: 0, route: '/sokak-olay-ekle' },
    { tur: 'Sosyal Medya', ikon: 'share', renk: '#8e44ad', sayi: 0, route: '/socialmedia' },
    { tur: 'Seçim', ikon: 'vote', renk: '#2980b9', sayi: 0, route: '/secim' },
    { tur: 'VIP Ziyaret', ikon: 'star', renk: '#f39c12', sayi: 0, route: '/vip' },
    { tur: 'Kuruluş', ikon: 'org', renk: '#27ae60', sayi: 0, route: '/organizasyon' },
    { tur: 'Konu', ikon: 'tag', renk: '#1a3a5c', sayi: 0, route: '/konu-islemleri' },
    { tur: 'Kullanıcı', ikon: 'users', renk: '#16a085', sayi: 0, route: '/kullanicilar' },
  ];

  readonly alanlar = [
    { baslik: 'Olay Yönetimi', aciklama: 'Tüm olay kayıtlarını görüntüle ve yönet.', route: '/olay', renk: '#e74c3c', etiket: 'Olaylar' },
    { baslik: 'Sokak Olayı Girişi', aciklama: 'Yeni sokak olayı ekle veya mevcut kayıtları düzenle.', route: '/sokak-olay-ekle', renk: '#e67e22', etiket: 'Veri Girişi' },
    { baslik: 'Sosyal Medya Olayları', aciklama: 'Sosyal medya kaynaklı olay takibi.', route: '/socialmedia', renk: '#8e44ad', etiket: 'Takip' },
    { baslik: 'Seçim Olayları', aciklama: 'Seçim dönemine ait olay ve faaliyetler.', route: '/secim', renk: '#2980b9', etiket: 'Seçim' },
    { baslik: 'VIP Ziyaretler', aciklama: 'Önemli ziyaret kayıtları ve protokol.', route: '/vip', renk: '#f39c12', etiket: 'Protokol' },
    { baslik: 'Kuruluş İşlemleri', aciklama: 'Organizasyon ve kuruluş bilgilerini yönet.', route: '/organizasyon', renk: '#27ae60', etiket: 'Yönetim' },
    { baslik: 'Konu İşlemleri', aciklama: 'Olaylar için konu ve kategori hiyerarşisi.', route: '/konu-islemleri', renk: '#1a3a5c', etiket: 'Yönetim' },
    { baslik: 'Kullanıcı Yönetimi', aciklama: 'Sistem kullanıcıları ve rol atamaları.', route: '/kullanicilar', renk: '#16a085', etiket: 'Yönetim' },
  ];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.verileriGetir();
  }

  verileriGetir(): void {
    this.yukleniyor = true;
    this.hata = null;

    const istekler: Promise<void>[] = [
      this.sayiGetir('/api/olay', 0),
      this.sayiGetir('/api/olay', 1),
      this.sayiGetir('/api/sosyalmedyaolay', 2),
      this.sayiGetir('/api/secim', 3),
      this.sayiGetir('/api/vipziyaret', 4),
      this.sayiGetir('/api/organizator', 5),
      this.sayiGetir('/api/organizator/konu', 6),
      this.sayiGetir('/api/user', 7),
    ];

    Promise.allSettled(istekler).then(() => {
      this.yukleniyor = false;
    });
  }

  private sayiGetir(endpoint: string, index: number): Promise<void> {
    return new Promise(resolve => {
      this.http.get<any[]>(`${this.baseUrl}${endpoint}`).subscribe({
        next: data => {
          if (Array.isArray(data)) this.ozet[index].sayi = data.length;
          resolve();
        },
        error: () => resolve()
      });
    });
  }

  tarihFormat(tarih: string): string {
    if (!tarih) return '-';
    return new Date(tarih).toLocaleDateString('tr-TR', {
      day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit'
    });
  }

  toplamKayit(): number {
    return this.ozet.slice(0, 5).reduce((t, o) => t + o.sayi, 0);
  }
}