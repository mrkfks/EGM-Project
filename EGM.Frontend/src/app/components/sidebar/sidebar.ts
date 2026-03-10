import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd  } from '@angular/router';
import { filter, Subscription } from 'rxjs';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css'],
})
export class Sidebar implements OnInit, OnDestroy {

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