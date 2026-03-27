import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { NotificationCenter } from '../notification-center/notification-center';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationCenter],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css'],
})
export class Navbar implements OnInit, OnDestroy {
  show = true;
  userName = 'Kullanıcı';
  userRole = '';
  userEmail = '';
  dropdownOpen = false;
  pageTitle    = 'EGM Proje';
  pageSubtitle = 'Güvenlik Yönetim Sistemi';
  private sub?: Subscription;

  private readonly PAGE_TITLES: Record<string, { title: string; subtitle: string }> = {
    '/home':            { title: 'Harita',                  subtitle: 'Operasyonel Durum Haritası' },
    '/olay':            { title: 'Olay Listesi',            subtitle: 'Sistemdeki tüm olayları görüntüleyin ve filtreleyin' },
    '/sokak-olay-ekle': { title: 'Sokak Olayı Ekle',        subtitle: 'Yeni sokak olayını sisteme kaydediniz' },
    '/vip':             { title: 'VIP Ziyaret Takibi',      subtitle: 'Devlet büyükleri ve protokol ziyaretleri' },
    '/organizasyon':    { title: 'Kuruluş İşlemleri',      subtitle: 'Sendikalar, konfederasyonlar ve sivil toplum kuruluşları' },
    '/secim':           { title: 'Seçim Güvenliği',        subtitle: 'Sandık güvenliği ihlalleri ve olay kaydı' },
    '/socialmedia':     { title: 'Sosyal Medya Olayları',  subtitle: 'Sosyal medya kaynaklı olaylar' },
    '/olu':             { title: 'Ölü Kayıtları',           subtitle: 'Ölü kayıt yönetimi' },
    '/supheli':         { title: 'Şüpheli Kayıtları',      subtitle: 'Şüpheli takip ve kayıt yönetimi' },
    '/operasyonel':     { title: 'Operasyonel Faaliyetler', subtitle: 'Operasyonel faaliyet kayıtları' },
    '/istatistikler':   { title: 'Güvenlik İstatistikleri', subtitle: 'Olay verilerinin istatistiksel analizi' },
    '/raporlar':        { title: 'Raporlar',                subtitle: 'Rapor oluşturma ve yönetimi' },
    '/konu-islemleri':  { title: 'Konu İşlemleri',         subtitle: 'Konu ve alt konu kategorileri yönetimi' },
    '/sehit':           { title: 'Şehit Kayıtları',         subtitle: 'Şehit kayıt yönetimi' },
    '/kullanicilar':    { title: 'Kullanıcı Yönetimi',     subtitle: 'Sistem kullanıcıları ve rol atamaları' },
    '/veri-yonetimi':   { title: 'Veri Yönetimi',          subtitle: 'Merkezi veri erişim noktası' },
  };

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.decodeToken();
    this.checkRoute(this.router.url);
    this.sub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe((e: any) => this.checkRoute(e.url));
  }

  ngOnDestroy(): void { this.sub?.unsubscribe(); }

  private checkRoute(url: string): void {
    this.show = !url.includes('/login');
    this.dropdownOpen = false;
    const match = Object.keys(this.PAGE_TITLES).find(k => url.startsWith(k));
    if (match) {
      this.pageTitle    = this.PAGE_TITLES[match].title;
      this.pageSubtitle = this.PAGE_TITLES[match].subtitle;
    } else {
      this.pageTitle    = 'EGM Proje';
      this.pageSubtitle = 'Güvenlik Yönetim Sistemi';
    }
  }

  private decodeToken(): void {
    try {
      const token = localStorage.getItem('token');
      if (!token) return;
      const payload = token.split('.')[1];
      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(Array.prototype.map.call(atob(base64), (c: string) => {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      const p = JSON.parse(json);
      this.userName = p['fullName'] ?? p['name'] ?? p['unique_name'] ?? '';
      this.userRole = p['role'] ?? p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? '';
      this.userEmail = p['email'] ?? '';
    } catch { }
  }

  get initials(): string {
    return this.userName
      .split(' ')
      .slice(0, 2)
      .map(w => w[0])
      .join('')
      .toUpperCase()
      || 'U';
  }

  get roleLabel(): string {
    const map: Record<string, string> = {
      BaskanlikYoneticisi: 'Başkanlık Yöneticisi',
      BaskanlikPersoneli: 'Başkanlık Personeli',
      IlYoneticisi: 'İl Yöneticisi',
      IlPersoneli: 'İl Personeli',
      Izleyici: 'İzleyici',
    };
    return map[this.userRole] ?? this.userRole;
  }

  toggleDropdown(): void { this.dropdownOpen = !this.dropdownOpen; }

  logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
}
