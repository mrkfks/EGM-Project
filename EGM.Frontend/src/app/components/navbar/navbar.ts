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
  private sub?: Subscription;

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
  }

  private decodeToken(): void {
    try {
      const token = localStorage.getItem('token');
      if (!token) return;
      const p = JSON.parse(atob(token.split('.')[1]));
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
