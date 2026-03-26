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
      title: 'Ana Ekranlar',
      items: [
        { label: 'Harita', route: '/home', icon: 'map' },
      ]
    },
    {
      title: 'Olaylar & Raporlar',
      items: [
        { label: 'Olaylar',                route: '/olay',        icon: 'alert' },
        { label: 'Operasyonel Faaliyetler', route: '/operasyonel', icon: 'activity' },
        { label: 'Sosyal Medya Olayları',  route: '/socialmedia', icon: 'share' },
        { label: 'Şüpheliler',             route: '/supheli',     icon: 'search-person' },
      ]
    },
    {
      title: 'Kayıtlar',
      items: [
        { label: 'Organizatörler',   route: '/organizasyon', icon: 'org' },
        { label: 'Seçim Sonuçları',  route: '/secim',        icon: 'vote' },
        { label: 'Şehitler',         route: '/sehit',        icon: 'medal' },
        { label: 'Ölüm Kayıtları',   route: '/olu',          icon: 'cross' },
        { label: 'VIP Ziyaretler',   route: '/vip',          icon: 'star' },
      ]
    },
    {
      title: 'Yönetim',
      items: [
        { label: 'Kullanıcı Yönetimi', route: '/kullanicilar', icon: 'users',
          roles: ['BaskanlikYoneticisi', 'IlYoneticisi'] },
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
      const payload = JSON.parse(atob(token.split('.')[1]));
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