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

// Rol sabitleri
const R = {
  Izleyici:           'Izleyici',
  IlPersoneli:        'IlPersoneli',
  IlYoneticisi:       'IlYoneticisi',
  BaskanlikPersoneli: 'BaskanlikPersoneli',
  BaskanlikYoneticisi:'BaskanlikYoneticisi',
  Yetkili:            'Yetkili',
};

// Kısa yardımcı diziler
const IL_P_VE_UZERI = [R.IlPersoneli, R.IlYoneticisi, R.BaskanlikPersoneli, R.BaskanlikYoneticisi, R.Yetkili];
const IL_Y_VE_UZERI = [R.IlYoneticisi, R.BaskanlikPersoneli, R.BaskanlikYoneticisi, R.Yetkili];
const HQ_Y_VE_UZERI = [R.BaskanlikYoneticisi, R.Yetkili];

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
      title: '',
      items: [
        { label: 'Harita',    route: '/harita',   icon: 'map' },
        { label: 'Ajanda',    route: '/ajanda',   icon: 'calendar' },
        { label: 'Bültenler', route: '/rapor-gunluk-bulten', icon: 'file-text' },
      ]
    },
    {
      title: '',
      items: [
        { label: 'İstatistikler', route: '/istatistik-paneli', icon: 'bar-chart' },
      ]
    },
    {
      title: '',
      items: [
        { label: 'Olay Bildirim Formları', route: '/olay-bildirim-paneli', icon: 'alert', roles: IL_P_VE_UZERI },
      ]
    },
    {
      title: '',
      items: [
        // İl Yöneticisi ve üzeri → tek panel
        { label: 'Yönetim Paneli',  route: '/yonetim-paneli', icon: 'settings', roles: IL_Y_VE_UZERI },
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