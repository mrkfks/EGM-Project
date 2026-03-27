import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';

export interface NavItem {
  label: string;
  route: string;
  icon: string;
  roles?: string[];
}

export interface NavGroup {
  title: string;
  items: NavItem[];
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css'],
})
export class Sidebar implements OnInit, OnDestroy {
  show = true;
  userRole = '';
  private sub?: Subscription;

  readonly groups: NavGroup[] = [
    {
      title: 'Ana Sayfa',
      items: [
        { label: 'Harita', route: '/home', icon: 'map' },
        { label: 'Olaylar', route: '/olay', icon: 'alert' },
        { label: 'İstatistikler', route: '/istatistikler', icon: 'activity' },
        { label: 'Raporlar', route: '/raporlar', icon: 'share' },
      ]
    },
    {
      title: 'Veri Girişi',
      items: [
        { label: 'Kuruluş İşlemleri', route: '/organizasyon', icon: 'org' },
        { label: 'Konu İşlemleri', route: '/konu-islemleri', icon: 'activity' },
        { label: 'Sokak Olayları', route: '/sokak-olay-ekle', icon: 'alert' },
        { label: 'Sosyal Medya Olayları', route: '/socialmedia', icon: 'share' },
        { label: 'Seçim Olayları', route: '/secim', icon: 'vote' },
        { label: 'Ziyaretçi', route: '/vip', icon: 'star' },
      ]
    },
    {
      title: 'Yönetim',
      items: [
        { label: 'Kullanıcı Yönetimi', route: '/kullanicilar', icon: 'users',
          roles: ['BaskanlikYoneticisi', 'IlYoneticisi'] },
        { label: 'Veri Yönetimi', route: '/veri-yonetimi', icon: 'database' },
      ]
    }
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.readRole();
    this.checkRoute(this.router.url);
    this.sub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe((e: any) => this.checkRoute(e.url));
  }

  ngOnDestroy(): void { this.sub?.unsubscribe(); }

  private checkRoute(url: string): void {
    this.show = !url.includes('/login');
  }

  private readRole(): void {
    try {
      const token = localStorage.getItem('token');
      if (!token) return;
      const payloadPart = token.split('.')[1];
      const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(Array.prototype.map.call(atob(base64), (c: string) => {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      const payload = JSON.parse(json);
      this.userRole = payload['role'] ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? '';
    } catch { this.userRole = ''; }
  }

  isVisible(item: NavItem): boolean {
    if (!item.roles || item.roles.length === 0) return true;
    return item.roles.includes(this.userRole);
  }

  isActive(route: string): boolean {
    return this.router.url === route || this.router.url.startsWith(route + '/');
  }

  visibleGroups(): NavGroup[] {
    return this.groups
      .map(g => ({ ...g, items: g.items.filter(i => this.isVisible(i)) }))
      .filter(g => g.items.length > 0);
  }
}