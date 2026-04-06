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
  pageTitle = '';
  pageIcon  = '';
  private sub?: Subscription;

  private readonly PAGE_MAP: Record<string, { title: string; icon: string }> = {
    '/home':                 { title: 'Harita',                         icon: 'map' },
    '/ajanda':               { title: 'Ajanda',                         icon: 'calendar' },
    '/olay':                 { title: 'Olay Listesi',                   icon: 'alert' },
    '/sokak-olay-ekle':      { title: 'Sokak Olayı Ekle',               icon: 'alert' },
    '/vip':                  { title: 'VIP Ziyaret Takibi',             icon: 'star' },
    '/organizasyon':         { title: 'Kuruluş İşlemleri',             icon: 'building' },
    '/secim':                { title: 'Seçim Güvenliği',               icon: 'vote' },
    '/socialmedia':          { title: 'Sosyal Medya Olayları',         icon: 'share' },
    '/olu':                  { title: 'Ölü Kayıtları',                  icon: 'activity' },
    '/supheli':              { title: 'Şüpheli Kayıtları',             icon: 'search-person' },
    '/operasyonel':          { title: 'Operasyonel Faaliyetler',        icon: 'activity' },
    '/istatistikler':        { title: 'Güvenlik İstatistikleri',        icon: 'bar-chart' },
    '/istatistik-paneli':    { title: 'İstatistikler',                  icon: 'bar-chart' },
    '/olay-bildirim-paneli': { title: 'Olay Bildirim Formları',         icon: 'alert' },
    '/yonetim-paneli':       { title: 'Yönetim Paneli',                icon: 'settings' },
    '/bulten-paneli':        { title: 'Bültenler',                      icon: 'file-text' },
    '/raporlar':             { title: 'Bültenler',                      icon: 'file-text' },
    '/rapor-gunluk-bulten':  { title: 'Başkanlık Günlük Bülten',       icon: 'file-text' },
    '/rapor-kuruluslar':     { title: 'Kuruluşlar',                     icon: 'bar-chart' },
    '/rapor-konular':        { title: 'Konular',                        icon: 'pie-chart' },
    '/konu-islemleri':       { title: 'Konu İşlemleri',                icon: 'book' },
    '/sehit':                { title: 'Şehit Kayıtları',               icon: 'star' },
    '/kullanicilar':         { title: 'Kullanıcı Yönetimi',            icon: 'users' },
    '/veri-yonetimi':        { title: 'Veri Yönetimi',                 icon: 'database' },
    '/sokak-istatistik':     { title: 'Sokak Olayları İstatistikleri',  icon: 'trending-up' },
    '/sosyal-medya-istatistik': { title: 'Sosyal Medya İstatistikleri', icon: 'hash' },
    '/secim-istatistik':     { title: 'Seçim Olayları İstatistikleri', icon: 'check-square' },
    '/vip-istatistik':       { title: 'Ziyaretçi Olayları İstatistikleri', icon: 'user-check' },
    '/konular':              { title: 'Konu İstatistikleri',            icon: 'pie-chart' },
    '/faaliyet-yonetimi':    { title: 'Faaliyet Yönetimi',             icon: 'briefcase' },
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
    const match = Object.keys(this.PAGE_MAP).find(k => url.startsWith(k));
    if (match) {
      this.pageTitle = this.PAGE_MAP[match].title;
      this.pageIcon  = this.PAGE_MAP[match].icon;
    } else {
      this.pageTitle = '';
      this.pageIcon  = '';
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
