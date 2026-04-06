import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Subject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface NotifItem {
  id?: string;
  title: string;
  message: string;
  hassasiyet: number;     // 0=Düşük, 1=Orta, 2=Yüksek, 3=Kritik
  riskNorm: number;       // hassasiyet / 3 (0–1 arası normalize değer)
  latitude?: number;
  longitude?: number;
  type: string;
  olayId: string;
  cityId?: number;
  timestamp: Date;
  isRead: boolean;
}

const API       = environment.apiUrl;
const MAX_NOTIF = 50;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly http       = inject(HttpClient);

  private _notifs     = new BehaviorSubject<NotifItem[]>([]);
  private _toast      = new Subject<NotifItem>();
  private _pulse      = new Subject<{ olayId: string; hassasiyet: number; latitude?: number; longitude?: number }>();

  readonly notifications$ = this._notifs.asObservable();
  readonly toast$         = this._toast.asObservable();
  readonly pulse$         = this._pulse.asObservable();
  readonly unreadCount$: Observable<number> = this._notifs.pipe(
    map(items => items.filter(n => !n.isRead).length)
  );

  private hubConn: any = null;
  private connecting   = false;

  /** Hub'a bağlan (idempotent). Guard ve her korumalı sayfadan çağrılabilir. */
  connect(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    if (this.hubConn || this.connecting)     return;
    const token = localStorage.getItem('token');
    if (!token) return;

    this.connecting = true;
    this.loadHistory();
    this.startHub();
  }

  private loadHistory(): void {
    this.http.get<any[]>(`${API}/api/notification`).subscribe({
      next: items => {
        const notifs: NotifItem[] = items.map(n => ({
          id:           n.id,
          title:        n.title        ?? '',
          message:      n.message      ?? '',
          hassasiyet:   0,
          riskNorm:     0,
          type:         n.type         ?? 'Risk',
          olayId:       n.id           ?? '',
          timestamp:    new Date(n.createdAt),
          isRead:       n.isRead       ?? false
        }));
        this._notifs.next(notifs.slice(0, MAX_NOTIF));
      },
      error: () => { /* sessizce devam et */ }
    });
  }

  private async startHub(): Promise<void> {
    try {
      const sr: any = await import('@microsoft/signalr');
      this.hubConn  = new sr.HubConnectionBuilder()
        .withUrl(`${API}/hubs/notifications`, {
          accessTokenFactory: () => localStorage.getItem('token') ?? ''
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 20000, 30000, 60000])
        .configureLogging(sr.LogLevel.Warning)
        .build();

      this.hubConn.on('ReceiveNotification', (data: any) => this.onIncoming(data));
      await this.hubConn.start();
    } catch {
      this.hubConn = null;
    } finally {
      this.connecting = false;
    }
  }

  private onIncoming(data: any): void {
    const item: NotifItem = {
      title:        data.title      ?? 'Bildirim',
      message:      data.message    ?? '',
      hassasiyet:   data.hassasiyet ?? 0,
      riskNorm:     (data.hassasiyet ?? 0) / 3,
      latitude:     data.latitude,
      longitude:    data.longitude,
      type:         data.type       ?? 'Risk',
      olayId:       String(data.olayId ?? ''),
      cityId:       data.cityId,
      timestamp:    new Date(),
      isRead:       false
    };

    const current = this._notifs.value;
    this._notifs.next([item, ...current].slice(0, MAX_NOTIF));
    this._toast.next(item);
    this._pulse.next({
      olayId:     item.olayId,
      hassasiyet: item.hassasiyet,
      latitude:   item.latitude,
      longitude:  item.longitude
    });
  }

  markAsRead(id: string): void {
    if (!id) return;
    this.http.post(`${API}/api/notification/${id}/read`, {}).subscribe({
      next: () => {
        const updated = this._notifs.value.map(n =>
          n.id === id ? { ...n, isRead: true } : n
        );
        this._notifs.next(updated);
      },
      error: () => {}
    });
  }

  markAllAsRead(): void {
    this.http.post(`${API}/api/notification/read-all`, {}).subscribe({
      next: () => {
        const updated = this._notifs.value.map(n => ({ ...n, isRead: true }));
        this._notifs.next(updated);
      },
      error: () => {}
    });
  }

  disconnect(): void {
    this.hubConn?.stop().catch(() => {});
    this.hubConn    = null;
    this.connecting = false;
  }
}
