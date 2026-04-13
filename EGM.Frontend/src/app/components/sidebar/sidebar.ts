import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { SidebarService } from '../../services/sidebar.service';

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
  isMobile = false;
  isOpen = false;
  private sub?: Subscription;
  private sidebarSub?: Subscription;

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

  constructor(private router: Router, private sidebarService: SidebarService) {
    this.checkMobileView();
  }

  ngOnInit(): void {
    this.checkMobileView();
    this.readRole();
    this.checkRoute(this.router.url);
    this.sub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe((e: any) => this.checkRoute(e.url));
    
    // Subscribe to sidebar service
    this.sidebarSub = this.sidebarService.isOpen$.subscribe((isOpen: boolean) => {
      this.isOpen = isOpen;
    });
  }

  ngOnDestroy(): void { 
    this.sub?.unsubscribe();
    this.sidebarSub?.unsubscribe();
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    this.checkMobileView();
  }

  private checkMobileView(): void {
    this.isMobile = window.innerWidth < 900;
    if (!this.isMobile) {
      this.sidebarService.closeSidebar(); // Desktop-da drawer'ı kapat
    }
  }

  toggleSidebar(): void {
    this.sidebarService.toggleSidebar();
  }

  closeSidebar(): void {
    this.sidebarService.closeSidebar();
  }

  private checkRoute(url: string): void {
    this.show = !url.includes('/login');
    if (this.isMobile) {
      this.sidebarService.closeSidebar(); // Rota değiştiğinde mobile drawer'ı kapat
    }
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