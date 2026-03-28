import { CommonModule, isPlatformBrowser } from '@angular/common';
import { environment } from '../../../environments/environment';
import {
  Component, Inject, PLATFORM_ID,
  AfterViewInit, OnDestroy, OnInit
} from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, takeUntil } from 'rxjs';
import { NotificationService } from '../../services/notification.service';
import type * as LeafletType from 'leaflet';

// ── Türkiye coğrafi sınırları ─────────────────────────────────────────────
const TURKEY_SW: [number, number] = [35.5, 25.5];
const TURKEY_NE: [number, number] = [42.5, 45.0];

// ── Hassasiyet renk paleti (0=Düşük … 3=Kritik) ───────────────────────────
export const HASSASIYET_COLORS: Record<number, string> = {
  0: '#27ae60',
  1: '#f39c12',
  2: '#e74c3c',
  3: '#8e44ad'
};
export const HASSASIYET_LABELS: Record<number, string> = {
  0: 'Düşük', 1: 'Orta', 2: 'Yüksek', 3: 'Kritik'
};
const DURUM_LABELS: Record<number, string> = {
  0: 'Planlanan', 1: 'Gerçekleşen', 2: 'İptal'
};
const HEAT_INTENSITY: Record<number, number> = {
  0: 0.25, 1: 0.55, 2: 0.80, 3: 1.00
};
const HEAT_RADIUS: Record<number, number> = {
  0: 20, 1: 30, 2: 40, 3: 52
};

// ── Veri modelleri ────────────────────────────────────────────────────────
export interface OlayMapItem {
  id: string;
  baslik: string;
  olayTuru?: string;
  tarih: string;
  il?: string;
  ilce?: string;
  latitude?: number;
  longitude?: number;
  hassasiyet: number;
  durum: number;
  riskPuani: number;
  katilimciSayisi?: number;
}

export interface HubNotification {
  title: string;
  message: string;
  riskPuani: number;
  type: string;
  olayId: string;
  hassasiyet?: number;
  latitude?: number;
  longitude?: number;
}

// ── Bileşen ───────────────────────────────────────────────────────────────
@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit, AfterViewInit, OnDestroy {

  // Leaflet
  private map?: LeafletType.Map;
  private L?: typeof LeafletType;
  private geoBounds?: LeafletType.LatLngBounds;
  private resizeObserver?: ResizeObserver;
  private resizeTimer?: number;
  private streetTile?: LeafletType.TileLayer;
  private satTile?: LeafletType.TileLayer;
  private markersGroup?: LeafletType.FeatureGroup;
  private heatLayer?: any;

  // Pulse map: olayId → CircleMarker SVG path elementi
  private pulseMap = new Map<string, SVGElement>();

  // UI state
  currentLayer: 'street' | 'satellite' | 'heatmap' = 'street';
  durumFilter = 'tum';
  zamanFilter = 'tum';
  isLoading   = false;
  errorMsg: string | null = null;

  // Bildirim banner
  activeNotif: HubNotification | null = null;
  notifVisible = false;
  private notifTimer?: number;

  // RBAC — JWT'den çözülen CityId
  tokenCityId: number | null = null;
  tokenRole:   string | null = null;

  // Data
  private olaylar: OlayMapItem[] = [];
  private destroy$ = new Subject<void>();

  // Template sabitleri
  readonly hassColors = HASSASIYET_COLORS;
  readonly hassLabels = HASSASIYET_LABELS;
  readonly hassKeys   = [0, 1, 2, 3];
  readonly apiBase    = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object,
    private notifSvc: NotificationService
  ) {}

  // ── Lifecycle ─────────────────────────────────────────────────────────

  ngOnInit(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    document.body.classList.add('no-scroll');
    this.decodeToken();
  }

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    try {
      await this.initMap();
      setTimeout(() => this.map?.invalidateSize(), 200);
    } catch (err) {
      console.error('[EGM] initMap() failed', err);
      this.errorMsg = 'Harita başlatılamadı. Lütfen sayfayı yenileyin.';
      return;
    }
    await this.loadTurkeyGeoJSON();
    await this.loadOlaylar();
    this.notifSvc.connect();

    // Harita pulse animasyonu
    this.notifSvc.pulse$.pipe(takeUntil(this.destroy$)).subscribe(({ olayId }) => {
      const pathEl = this.pulseMap.get(olayId);
      if (pathEl) {
        pathEl.classList.add('egm-pulse');
        setTimeout(() => pathEl.classList.remove('egm-pulse'), 3200);
      }
    });

    // Home banner bildirimi (reload + üst banner)
    this.notifSvc.toast$.pipe(takeUntil(this.destroy$)).subscribe(n => {
      this.handleNotif({ ...n, riskPuani: n.riskPuaniRaw });
    });
  }

  ngOnDestroy(): void {
    if (isPlatformBrowser(this.platformId)) {
      document.body.classList.remove('no-scroll');
    }
    this.resizeObserver?.disconnect();
    if (this.resizeTimer)  window.clearTimeout(this.resizeTimer);
    if (this.notifTimer)   window.clearTimeout(this.notifTimer);
    this.map?.remove();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Harita başlatma ────────────────────────────────────────────────────

  private async initMap(): Promise<void> {
    console.log('[EGM] initMap() called');
    const leafletModule = await import('leaflet');
    // CJS modüller esbuild ile sarmalandığında gerçek namespace "default" altında gelir.
    // Her iki durumu (ESM doğrudan / CJS-wrapped) karşılayan güvenli erişim:
    const L = ((leafletModule as any).default ?? leafletModule) as typeof LeafletType;
    console.log('[EGM] leaflet namespace resolved, L.map type:', typeof L.map);
    this.L = L;

    L.Icon.Default.mergeOptions({
      iconUrl:       'assets/marker-icon.png',
      iconRetinaUrl: 'assets/marker-icon-2x.png',
      shadowUrl:     'assets/marker-shadow.png'
    });

    const maxBounds = L.latLngBounds(
      L.latLng(TURKEY_SW[0] - 4, TURKEY_SW[1] - 6),
      L.latLng(TURKEY_NE[0] + 4, TURKEY_NE[1] + 6)
    );

    this.map = L.map('map', {
      center: [39, 35], zoom: 6,
      minZoom: 5, maxZoom: 14,
      maxBounds, maxBoundsViscosity: 1.0,
      zoomControl: true, scrollWheelZoom: true
    });

    this.streetTile = L.tileLayer(
      'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
      { attribution: '© <a href="https://osm.org/copyright">OSM</a>', maxZoom: 19 }
    );
    this.satTile = L.tileLayer(
      'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
      { attribution: '© Esri', maxZoom: 19 }
    );
    this.streetTile.addTo(this.map);
    this.markersGroup = L.featureGroup().addTo(this.map);
    console.log('[EGM] Leaflet map initialized');

    const container = this.map.getContainer();
    this.resizeObserver = new ResizeObserver(() => {
      if (this.resizeTimer) window.clearTimeout(this.resizeTimer);
      this.resizeTimer = window.setTimeout(() => {
        this.map?.invalidateSize();
        this.fitToScreen();
      }, 100);
    });
    this.resizeObserver.observe(container);
  }

  // ── Türkiye GeoJSON ────────────────────────────────────────────────────

  private loadTurkeyGeoJSON(): Promise<void> {
    return new Promise(resolve => {
      if (!this.map || !this.L) { resolve(); return; }
      const L = this.L;
      const small  = window.innerWidth <= 768;
      const url    = small
        ? '/assets/turkey-provinces-simplified-low.geojson'
        : '/assets/turkey-provinces-simplified.geojson';
      const fbUrl  = '/assets/turkey-provinces-simplified.geojson';

      const load = (path: string) => {
        this.http.get(path).subscribe({
          next: (geo: any) => {
            const geoLayer = L.geoJSON(geo, {
              style: () => ({
                color: '#0b3954', weight: 1.2,
                fillColor: '#1b4f72', fillOpacity: 0.40
              }),
              onEachFeature: (feat: any, lyr: any) => {
                const name = feat?.properties?.Name
                          || feat?.properties?.name
                          || feat?.properties?.NAME_1 || 'İl';
                lyr.bindTooltip(name, { sticky: true });
                lyr.on({
                  mouseover: () => (lyr as any)._path?.classList?.add('hovered'),
                  mouseout:  () => {
                    (lyr as any)._path?.classList?.remove('hovered');
                    geoLayer.resetStyle(lyr);
                  },
                  click: () => this.onProvinceClick(name, feat)
                });
              }
            }).addTo(this.map!);

            this.geoBounds = geoLayer.getBounds();
            this.fitToScreen();
            requestAnimationFrame(() => requestAnimationFrame(() => this.fitToScreen()));
            setTimeout(() => this.fitToScreen(), 300);
            resolve();
          },
          error: () => { if (path !== fbUrl) load(fbUrl); else resolve(); }
        });
      };
      load(url);
    });
  }

  // ── Veri yükleme ───────────────────────────────────────────────────────

  async loadOlaylar(): Promise<void> {
    this.isLoading = true;
    this.errorMsg  = null;
    const params   = this.buildApiParams();

    this.http.get<any>(`${this.apiBase}/api/olay`, { params }).subscribe({
      next: (res: any) => {
        this.olaylar   = (res?.items ?? []) as OlayMapItem[];
        this.isLoading = false;
        this.renderMarkers();
      },
      error: (err) => {
        console.error('[EGM] loadOlaylar error', err);
        this.errorMsg  = 'Olay verileri yüklenemedi.';
        this.isLoading = false;
      }
    });
  }

  private buildApiParams(): Record<string, string> {
    const p: Record<string, string> = { sayfa: '1', sayfaBoyutu: '500' };

    if (this.durumFilter !== 'tum') p['durum'] = this.durumFilter;

    const now = new Date();
    if (this.zamanFilter === '24h') {
      p['tarihBaslangic'] = now.toISOString();
      p['tarihBitis']     = new Date(+now + 86_400_000).toISOString();
    } else if (this.zamanFilter === '7gun') {
      p['tarihBaslangic'] = now.toISOString();
      p['tarihBitis']     = new Date(+now + 7 * 86_400_000).toISOString();
    } else if (this.zamanFilter === 'gecmis') {
      p['tarihBitis'] = now.toISOString();
    }
    return p;
  }

  // ── Marker / Heatmap render ────────────────────────────────────────────

  private renderMarkers(): void {
    if (!this.markersGroup || !this.L) return;

    this.markersGroup.clearLayers();
    this.pulseMap.clear();

    if (this.heatLayer) {
      this.map?.removeLayer(this.heatLayer);
      this.heatLayer = undefined;
    }

    const items = this.olaylar.filter(o => o.latitude != null && o.longitude != null);

    if (this.currentLayer === 'heatmap') {
      this.renderHeatmap(items);
    } else {
      this.renderPins(items);
    }
  }

  private renderPins(items: OlayMapItem[]): void {
    const L = this.L!;
    items.forEach(item => {
      const color     = HASSASIYET_COLORS[item.hassasiyet] ?? '#aaa';
      const isPlanned = item.durum === 0;

      let marker: LeafletType.CircleMarker | LeafletType.Marker;

      if (isPlanned) {
        // Daire — planlanan olay
        marker = L.circleMarker([item.latitude!, item.longitude!], {
          radius: 9, color, fillColor: color, fillOpacity: 0.88, weight: 2
        });
        // Pulse için SVG yolunu kaydet (marker haritaya eklenince)
        marker.on('add', () => {
          const path = ((marker as any)._path) as SVGElement | undefined;
          if (path) this.pulseMap.set(item.id, path);
        });
      } else {
        // Pin ikonu — gerçekleşen olay
        const icon = L.divIcon({
          className: '',
          html: `<div class="egm-pin" style="--pc:${color}"></div>`,
          iconSize: [18, 26], iconAnchor: [9, 26]
        });
        marker = L.marker([item.latitude!, item.longitude!], { icon });
      }

      marker.bindPopup(this.buildPopup(item), { maxWidth: 310, className: 'egm-leaflet-popup' });
      this.markersGroup!.addLayer(marker);
    });
  }

  private async renderHeatmap(items: OlayMapItem[]): Promise<void> {
    if (!items.length || !this.map || !this.L) return;

    try {
      // leaflet.heat kuruluysa gerçek heatmap kullan
      const heatModule: any = await import('leaflet.heat');
      void heatModule;
      const pts = items.map(i => [i.latitude!, i.longitude!, HEAT_INTENSITY[i.hassasiyet] ?? 0.4] as [number, number, number]);
      this.heatLayer = (this.L as any).heatLayer(pts, {
        radius: 40, blur: 30, maxZoom: 14,
        gradient: { 0.25: '#27ae60', 0.55: '#f39c12', 0.80: '#e74c3c', 1.00: '#8e44ad' }
      }).addTo(this.map);
    } catch {
      // Fallback: büyük yarı şeffaf daireler (paket kurulu değilse)
      const L = this.L!;
      items.forEach(i => {
        const color = HASSASIYET_COLORS[i.hassasiyet] ?? '#aaa';
        L.circleMarker([i.latitude!, i.longitude!], {
          radius: HEAT_RADIUS[i.hassasiyet] ?? 25,
          color, fillColor: color, fillOpacity: 0.16, weight: 0
        }).addTo(this.markersGroup!);
      });
    }
  }

  // ── Popup içeriği ──────────────────────────────────────────────────────

  private buildPopup(item: OlayMapItem): string {
    const c    = HASSASIYET_COLORS[item.hassasiyet] ?? '#ccc';
    const hl   = HASSASIYET_LABELS[item.hassasiyet] ?? '-';
    const dl   = DURUM_LABELS[item.durum]           ?? '-';
    const date = new Date(item.tarih).toLocaleDateString('tr-TR', {
      day: '2-digit', month: 'long', year: 'numeric'
    });
    const yer = [item.il, item.ilce].filter(Boolean).join(' / ');
    const kp  = item.katilimciSayisi
      ? `<div class="pr"><span>Katılımcı</span><span>${item.katilimciSayisi.toLocaleString('tr-TR')}</span></div>` : '';
    const rp  = item.riskPuani > 0
      ? `<div class="pr"><span>Risk Puanı</span><span>${item.riskPuani.toFixed(1)}</span></div>` : '';

    return `<div class="egm-pop">
  <div class="egm-pop-head" style="border-left:4px solid ${c}">
    <b>${item.baslik ?? 'Olay'}</b>
    ${item.olayTuru ? `<small>${item.olayTuru}</small>` : ''}
  </div>
  <div class="egm-pop-body">
    <div class="pr"><span>Tarih</span><span>${date}</span></div>
    ${yer ? `<div class="pr"><span>Yer</span><span>${yer}</span></div>` : ''}
    <div class="pr"><span>Hassasiyet</span><span style="color:${c};font-weight:700">${hl}</span></div>
    <div class="pr"><span>Durum</span><span>${dl}</span></div>
    ${kp}${rp}
  </div>
</div>`;
  }

  // ── Katman seçici ──────────────────────────────────────────────────────

  setLayer(layer: 'street' | 'satellite' | 'heatmap'): void {
    if (!this.map) return;

    if (this.streetTile && this.map.hasLayer(this.streetTile)) this.map.removeLayer(this.streetTile);
    if (this.satTile    && this.map.hasLayer(this.satTile))    this.map.removeLayer(this.satTile);

    this.currentLayer = layer;
    (layer === 'street' ? this.streetTile : this.satTile)?.addTo(this.map);
    this.renderMarkers();
  }

  // ── Filtreler ──────────────────────────────────────────────────────────

  onDurumChange(e: Event): void {
    this.durumFilter = (e.target as HTMLSelectElement).value;
  }

  onZamanChange(e: Event): void {
    this.zamanFilter = (e.target as HTMLSelectElement).value;
  }

  applyFilters(): void {
    this.loadOlaylar();
  }

  // ── SignalR ────────────────────────────────────────────────────────────

  private handleNotif(n: HubNotification): void {
    this.activeNotif  = n;
    this.notifVisible = true;

    if (this.notifTimer) window.clearTimeout(this.notifTimer);
    this.notifTimer = window.setTimeout(() => { this.notifVisible = false; }, 7000);

    // Olay listede yoksa veriyi yenile
    if (!this.olaylar.find(o => o.id === n.olayId)) {
      this.loadOlaylar();
    }
  }

  dismissNotif(): void {
    this.notifVisible = false;
  }

  // ── İl tıklama ────────────────────────────────────────────────────────

  onProvinceClick(name: string, feature: any): void {
    const id = feature?.properties?.id
            || feature?.properties?.NAME_1
            || feature?.properties?.name || name;
    this.router.navigate(['/province', encodeURIComponent(String(id))]);
  }

  // ── Yardımcılar ───────────────────────────────────────────────────────

  private fitToScreen(): void {
    if (!this.map || !this.geoBounds) return;
    this.map.invalidateSize();
    this.map.fitBounds(this.geoBounds, { padding: [10, 10], animate: false });
  }

  private decodeToken(): void {
    try {
      const tok = localStorage.getItem('token');
      if (!tok) return;
      const payloadPart = tok.split('.')[1];
      const base64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(Array.prototype.map.call(atob(base64), (c: string) => {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
      }).join(''));
      const payload = JSON.parse(json);
      this.tokenCityId = payload.cityId ?? payload.CityId ?? null;
      this.tokenRole   = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
                      ?? payload.role ?? null;
    } catch { /* Geçersiz token formatı — yoksay */ }
  }

  get markerCount(): number {
    return this.olaylar.filter(o => o.latitude != null && o.longitude != null).length;
  }
}
