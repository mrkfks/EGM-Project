import {
  Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { NotificationService, NotifItem } from '../../services/notification.service';

interface ToastEntry { id: number; item: NotifItem; closing: boolean; }

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-center.html',
  styleUrl: './notification-center.css',
  changeDetection: ChangeDetectionStrategy.Default
})
export class NotificationCenter implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private toastSeq = 0;

  panelOpen = false;
  toasts: ToastEntry[] = [];

  constructor(
    public notifSvc: NotificationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.notifSvc.toast$.pipe(takeUntil(this.destroy$)).subscribe(item => {
      this.addToast(item);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  togglePanel(): void { this.panelOpen = !this.panelOpen; }
  closePanel():  void { this.panelOpen = false; }

  onMarkAsRead(item: NotifItem, event: Event): void {
    event.stopPropagation();
    if (item.id) this.notifSvc.markAsRead(item.id);
  }

  onMarkAllRead(): void { this.notifSvc.markAllAsRead(); }

  dismissToast(id: number): void {
    const t = this.toasts.find(x => x.id === id);
    if (!t) return;
    t.closing = true;
    this.cdr.markForCheck();
    setTimeout(() => {
      this.toasts = this.toasts.filter(x => x.id !== id);
      this.cdr.markForCheck();
    }, 400);
  }

  hassasiyetColor(h: number): string {
    const map: Record<number, string> = { 0: '#27ae60', 1: '#f39c12', 2: '#e74c3c', 3: '#8e44ad' };
    return map[h] ?? '#555';
  }

  hassasiyetLabel(h: number): string {
    const map: Record<number, string> = { 0: 'Düşük', 1: 'Orta', 2: 'Yüksek', 3: 'Kritik' };
    return map[h] ?? '—';
  }

  timeAgo(d: Date): string {
    const diff = Math.floor((Date.now() - new Date(d).getTime()) / 1000);
    if (diff < 60)    return `${diff}sn`;
    if (diff < 3600)  return `${Math.floor(diff / 60)}dk`;
    if (diff < 86400) return `${Math.floor(diff / 3600)}sa`;
    return `${Math.floor(diff / 86400)}g`;
  }

  private addToast(item: NotifItem): void {
    if (this.toasts.length >= 5) this.dismissToast(this.toasts[0].id);
    const id = ++this.toastSeq;
    this.toasts.push({ id, item, closing: false });
    this.cdr.markForCheck();
    setTimeout(() => this.dismissToast(id), 10000);
  }
}
