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
  private sub?: Subscription;

  constructor(private router: Router) { }

  ngOnInit(): void {
    this.checkRoute(this.router.url);
    this.sub = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.checkRoute(event.url);
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private checkRoute(url: string): void {
    this.show = url !== '/login';
  }

}
