import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import {
  Component, Inject, OnInit, OnDestroy, PLATFORM_ID, signal
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription, firstValueFrom } from 'rxjs';
import { MapDataService } from '../../services/map-data.service';

function getFeatureName(props: any, fallback: string): string {
  return props?.Name || props?.name || props?.NAME_1 || props?.NAME_2 || fallback;
}

function normalizeKeyLocal(s: string | undefined): string {
  if (!s) return '';
  return String(s)
    .toLowerCase()
    .replace(/ç/g, 'c')
    .replace(/ğ/g, 'g')
    .replace(/ı/g, 'i')
    .replace(/İ/g, 'i')
    .replace(/ö/g, 'o')
    .replace(/ş/g, 's')
    .replace(/ü/g, 'u')
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '')
    .trim();
}

const ABBREV_DISPLAY: Record<string,string> = {
  'kmaras': 'Kahramanmaraş'
};

// Aliases for matching short forms in feature properties
const ABBREV_ALIASES_LOCAL: Record<string,string> = {
  'kmaras': 'kahramanmaras'
};

// ── Stil sabitleri ────────────────────────────────────────────────────────────
const S_COUNTRY      = { color: '#b0b8c0', weight: 0.5, fillColor: '#e8ecef', fillOpacity: 1.0 };
const S_COUNTRY_HVR  = { fillColor: '#dce1e5', fillOpacity: 1.0 };
const S_SEA          = '#a8cfe8';    // CSS background (deniz rengi)

const S_PROV_DEF    = { color: '#1a5276', weight: 1.2, fillColor: '#d6eaf8', fillOpacity: 0.92 };
const S_PROV_HVR    = { fillColor: '#aed6f1', fillOpacity: 0.95 };

// İlçe stilleri
const S_DIST_DEF    = { color: '#1a7a6e', weight: 0.9,  fillColor: '#d1f2eb', fillOpacity: 0.88 };
const S_DIST_HVR    = { fillColor: '#a2d9ce', fillOpacity: 0.95 };
const S_DIST_SEL    = { color: '#1a5276', weight: 2.0,  fillColor: '#7fb3d3', fillOpacity: 0.85 };



@Component({
  selector: 'app-province',
  standalone: true,
  imports: [CommonModule],
  template: `
<div class="egm-map-wrap">

  <!-- ═══ Araç Çubuğu ══════════════════════════════════════════════════════ -->
  <div class="egm-toolbar">
    <button class="toolbar-btn" (click)="goHome()" title="Ana Menüye Dön">
      <span class="tb-icon">⬅</span> Ana Menü
    </button>

    <div class="egm-breadcrumb">
      <span class="bc-item" [class.bc-link]="!!selectedProvinceId()" (click)="clearSelection()">🗺 Türkiye</span>
      <ng-container *ngIf="selectedProvinceId()">
        <span class="bc-sep">›</span>
        <span class="bc-item bc-active">{{ provinceName() }}</span>
      </ng-container>
      <ng-container *ngIf="selectedDistrictName()">
        <span class="bc-sep">›</span>
        <span class="bc-item bc-active">{{ selectedDistrictName() }}</span>
      </ng-container>
    </div>

    <select class="egm-select" (change)="selectProvince($event)">
      <option value="">🔍 İl seçin...</option>
      <option *ngFor="let p of provinces" [value]="p.id" [selected]="p.id === selectedProvinceId()">{{ p.name }}</option>
    </select>
  </div>

  <!-- ═══ Harita ════════════════════════════════════════════════════════════ -->
  <div id="provinceMap" class="egm-map-canvas"></div>

  <!-- ═══ Bilgi Paneli ═════════════════════════════════════════════════════ -->
  <div class="egm-info-panel" *ngIf="selectedProvinceId()">
    <div class="ip-header">
      <span class="ip-icon">📍</span>
      <strong class="ip-title">{{ provinceName() }}</strong>
    </div>
    <div class="ip-body">
      <div class="ip-stat">
        <span class="ip-label">İlçe Sayısı</span>
        <span class="ip-value">{{ districtCount() }}</span>
      </div>
      <div class="ip-stat" *ngIf="selectedDistrictName()">
        <span class="ip-label">Seçili İlçe</span>
        <span class="ip-value ip-highlight">{{ selectedDistrictName() }}</span>
      </div>
    </div>
    <button class="ip-clear-btn" (click)="clearSelection()">✕ Seçimi Temizle</button>
  </div>

  <!-- ═══ Lejand ════════════════════════════════════════════════════════════ -->
  <div class="egm-legend">
    <div class="leg-title">Lejand</div>
    <div class="leg-row"><span class="leg-box" style="background:#d6eaf8;border:1.5px solid #1a5276"></span>İl Sınırı</div>
    <div class="leg-row"><span class="leg-box" style="background:#d1f2eb;border:1.5px solid #1a7a6e"></span>İlçe Sınırı</div>
    <div class="leg-row"><span class="leg-box" style="background:#f5eef8;border:1.5px solid #7d3c98"></span>Mahalle (OSM)</div>
    <div class="leg-row"><span class="leg-box" style="background:#f1948a;border:2px solid #922b21"></span>Seçili İl</div>
    <div class="leg-row"><span class="leg-box" style="background:#dde3e7;border:1px solid #9eaab5"></span>Komşu Ülke</div>
    <div class="leg-row"><span class="leg-box" style="background:#a8cfe8;border:1px solid #7aaec8"></span>Deniz</div>
  </div>

  <!-- ═══ Yükleniyor göstergesi ════════════════════════════════════════════ -->
  <div class="egm-map-loading" *ngIf="loading()">
    <span class="loading-spinner"></span> Harita yükleniyor...
  </div>

</div>
  `,
  host: { style: 'display:block;position:absolute;inset:0;' }
})
export class Province implements OnInit, OnDestroy {

  // ── Açık state ─────────────────────────────────────────────────────────────
  readonly provinceName         = signal('');
  readonly selectedProvinceId   = signal<string | null>(null);
  readonly selectedDistrictName = signal<string | null>(null);
  readonly districtCount        = signal(0);
  readonly loading              = signal(true);
  provinces: Array<{ id: string; name: string }> = [];

  // ── Özel ───────────────────────────────────────────────────────────────────
  private map:            any;
  private L:              any;
  private features:       any[] = [];
  private countriesLayer: any = null;
  private provincesLayer: any = null;
  private highlightLayer: any = null;
  private districtLayer:  any = null;
  private mahalleLayer:   any = null;
  private locateMarker:   any = null;
  private subs = new Subscription();
  private kbHandler: ((e: KeyboardEvent) => void) | null = null;

  constructor(
    private route:    ActivatedRoute,
    private router:   Router,
    @Inject(PLATFORM_ID) private platformId: Object,
    private mapData:  MapDataService,
    private http:     HttpClient
  ) {}

  // ──────────────────────────────────────────────────────────────────────────
  async ngOnInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;

    const L = await import('leaflet');
    this.L = L;

    // Leaflet default icon yolları
    (L as any).Icon.Default.mergeOptions({
      iconUrl:       'assets/marker-icon.png',
      iconRetinaUrl: 'assets/marker-icon-2x.png',
      shadowUrl:     'assets/marker-shadow.png',
    });

    // ── Harita oluştur ──────────────────────────────────────────────────────
    this.map = L.map('provinceMap', {
      center:           [39, 35],
      zoom:             6,
      minZoom:          3,
      maxZoom:          14,
      zoomControl:      false,
      attributionControl: false,
      scrollWheelZoom:  true,
      doubleClickZoom:  true,
      touchZoom:        true,
      dragging:         true,
      boxZoom:          true,
      keyboard:         true,
    });

    (this.map.getContainer() as HTMLElement).style.background = S_SEA;

    // ── Kontroller ──────────────────────────────────────────────────────────
    L.control.zoom({ position: 'topright' }).addTo(this.map);
    L.control.scale({ position: 'bottomleft', metric: true, imperial: false }).addTo(this.map);
    L.control.attribution({ prefix: '© Yerel Veri', position: 'bottomright' }).addTo(this.map);

    this.addFullscreenControl(L);
    this.addLocateControl(L);

    // ── Klavye: Escape → seçimi sıfırla ────────────────────────────────────
    this.kbHandler = (e: KeyboardEvent) => { if (e.key === 'Escape') this.clearSelection(); };
    document.addEventListener('keydown', this.kbHandler);

    // ── Komşu ülkeler katmanı (yerel GeoJSON) ──────────────────────────────
    try {
      // Öncelikle küresel ülkeler dosyası varsa onu kullan (daha kapsamlı)
      let geo: any = null;
      try {
        geo = await firstValueFrom(this.http.get<any>('/assets/world-countries.geojson'));
      } catch (_) {
        // fallback: küçük bölgesel dosya
        geo = await firstValueFrom(this.http.get<any>('/assets/nearby-countries.geojson'));
      }

      this.countriesLayer = L.geoJSON(geo.features, {
        style:          () => ({ ...S_COUNTRY }),
        onEachFeature:  (f: any, layer: any) => {
          const nm = f.properties?.name || f.properties?.ADMIN || f.properties?.name_local || '';
          if (nm) layer.bindTooltip(nm, { direction: 'center', className: 'map-country-label', sticky: true });
          layer.on({
            mouseover: () => layer.setStyle(S_COUNTRY_HVR),
            mouseout:  () => layer.setStyle(S_COUNTRY),
          });
        },
      }).addTo(this.map);
      this.countriesLayer.bringToBack();
    } catch (e) {
      console.warn('Komşu ülke katmanı yüklenemedi', e);
    }

    // ── İl verileri ─────────────────────────────────────────────────────────
    this.subs.add(
      this.mapData.provinces$.subscribe({
        next: (features: any[]) => {
          this.features = features;
          this.provinces = features
            .map((f: any) => {
              const n = getFeatureName(f.properties, '');
              if (!n) return null;
              const norm = normalizeKeyLocal(n);
              const display = ABBREV_DISPLAY[norm] ?? n;
              return { id: String(display), name: String(display) };
            })
            .filter((x): x is { id: string; name: string } => !!x)
            .sort((a, b) => a.name.localeCompare(b.name, 'tr'));

          this.renderProvinces(features);
          this.loading.set(false);

          // Rota parametresi izle
          this.subs.add(
            this.route.paramMap.subscribe(params => {
              const id = decodeURIComponent(params.get('id') || '');
              this.highlightProvince(id);
            })
          );
        },
        error: (e: any) => {
          console.warn('İl GeoJSON yüklenemedi', e);
          this.loading.set(false);
        },
      })
    );
  }

  // ── Tüm iller katmanı ────────────────────────────────────────────────────
  private renderProvinces(features: any[]): void {
    const L = this.L;
    if (this.provincesLayer) { try { this.map.removeLayer(this.provincesLayer); } catch (_) {} }

    this.provincesLayer = L.geoJSON(features, {
      style: () => ({ ...S_PROV_DEF }),
      onEachFeature: (f: any, layer: any) => {
        const name = getFeatureName(f.properties, 'İl');
        layer.bindTooltip(name, { direction: 'center', className: 'map-tooltip', sticky: false });
        layer.on({
          mouseover: (_e: any) => {
            if (this.selectedProvinceId() !== name) layer.setStyle(S_PROV_HVR);
          },
          mouseout:  (_e: any) => {
            if (this.selectedProvinceId() !== name) layer.setStyle(S_PROV_DEF);
          },
          click:     () => {
            this.router.navigate(['/province', encodeURIComponent(name)]);
          },
        });
      },
    }).addTo(this.map);
  }

  // ── İl vurgulama ─────────────────────────────────────────────────────────
  private highlightProvince(id: string): void {
    const L = this.L;
    if (!L || !this.map) return;

    this.removeLayer(this.highlightLayer);
    this.highlightLayer = null;
    this.removeDistrictLayer();  // provincesLayer'ı haritaya geri ekler ve stilini sıfırlar

    if (!id) {
      this.selectedProvinceId.set(null);
      this.provinceName.set('');
      this.selectedDistrictName.set(null);
      this.districtCount.set(0);
      this.fitAll();
      return;
    }

    const idNorm = normalizeKeyLocal(id);
    // Try exact match first (existing behavior)
    let match = this.features.find((f: any) => {
      const p = f.properties || {};
      return p.Name === id || p.name === id || p.NAME_1 === id ||
             normalizeKeyLocal(p.Name || p.name || p.NAME_1 || '') === idNorm;
    });

    if (!match) {
      // Try normalized match: compare normalized feature names
      match = this.features.find((f: any) => {
        const name = getFeatureName(f.properties, '');
        const featNorm = normalizeKeyLocal(name);
        if (featNorm === idNorm) return true;
        // If feature has a short-form alias (e.g. 'kmaras'), map to expanded and compare
        const aliasMapped = ABBREV_ALIASES_LOCAL[featNorm];
        if (aliasMapped && aliasMapped === idNorm) return true;
        return false;
      });
    }

    if (match) {
      // provincesLayer ve countriesLayer'ı haritadan kaldır — alttan taşma olmaz
      if (this.provincesLayer && this.map.hasLayer(this.provincesLayer)) {
        this.map.removeLayer(this.provincesLayer);
      }
      if (this.countriesLayer && this.map.hasLayer(this.countriesLayer)) {
        this.map.removeLayer(this.countriesLayer);
      }

      // Bounds için geçici katman (GADM outline çizmiyoruz — ilçe outer rings kullanacağız)
      this.removeLayer(this.highlightLayer);
      this.highlightLayer = null;
      const tmpLayer = L.geoJSON(match);
      const bounds = tmpLayer.getBounds();
      if (bounds?.isValid()) {
        this.map.fitBounds(bounds, { padding: [40, 40], animate: true, maxZoom: 10 });
      }

      this.provinceName.set(getFeatureName(match.properties, id));
      this.selectedProvinceId.set(id);
      this.loadDistricts(this.provinceName());
    } else {
      this.fitAll();
    }
  }

  // ── İlçe yükleme ─────────────────────────────────────────────────────────
  private loadDistricts(provinceName: string): void {
    if (!provinceName) return;
    this.subs.add(
      this.mapData.getDistrictsForProvince(provinceName).subscribe({
        next: (features: any[]) => {
          this.districtCount.set(features.length);
          if (features.length > 0) this.renderDistricts(features);
        },
        error: (e: any) => console.warn('İlçe yükleme hatası', e),
      })
    );
  }

  // ── İlçe katmanı ─────────────────────────────────────────────────────────
  private renderDistricts(features: any[]): void {
    if (!this.L || !this.map) return;
    this.removeDistrictLayer();
    const L = this.L;

    this.districtLayer = L.geoJSON(features, {
      style: () => ({ ...S_DIST_DEF }),
      onEachFeature: (f: any, layer: any) => {
        const name = f.properties?.NAME_2 || getFeatureName(f.properties, 'İlçe');
        layer.bindTooltip(name, { direction: 'center', className: 'map-tooltip' });
        layer.on({
          mouseover: () => { if (this.selectedDistrictName() !== name) layer.setStyle(S_DIST_HVR); },
          mouseout:  () => { if (this.selectedDistrictName() !== name) layer.setStyle(S_DIST_DEF); },
          click:     () => {
            if (this.selectedDistrictName()) {
              this.districtLayer?.eachLayer((l: any) => l.setStyle(S_DIST_DEF));
            }
            layer.setStyle(S_DIST_SEL);
            this.selectedDistrictName.set(name);
            try {
              const b = layer.getBounds();
              if (b?.isValid()) this.map.fitBounds(b, { padding: [40, 40], animate: true });
            } catch (_) {}
            this.loadMahallesFromOSM(f);
          },
        });
      },
    }).addTo(this.map);

  }

  // ── Seçimi temizle ───────────────────────────────────────────────────────
  clearSelection(): void {
    this.router.navigate(['/province']);
  }

  // ── Kontrol: Tam Ekran ───────────────────────────────────────────────────
  private addFullscreenControl(L: any): void {
    const FullscreenCtrl = (L.Control as any).extend({
      options: { position: 'topright' },
      onAdd(map: any) {
        const c = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
        const a = L.DomUtil.create('a', '', c);
        a.href = '#'; a.title = 'Tam Ekran'; a.innerHTML = '⛶';
        a.setAttribute('role', 'button');
        L.DomEvent.on(a, 'click', L.DomEvent.stopPropagation)
          .on(a, 'click', L.DomEvent.preventDefault)
          .on(a, 'click', () => {
            if (!document.fullscreenElement) {
              (map.getContainer() as any).requestFullscreen?.();
            } else {
              document.exitFullscreen?.();
            }
          });
        return c;
      },
    });
    new FullscreenCtrl().addTo(this.map);
  }

  // ── Kontrol: Konumum ─────────────────────────────────────────────────────
  private addLocateControl(L: any): void {
    const self = this;
    const LocateCtrl = (L.Control as any).extend({
      options: { position: 'topright' },
      onAdd(map: any) {
        const c = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
        const a = L.DomUtil.create('a', '', c);
        a.href = '#'; a.title = 'Konumumu Göster'; a.innerHTML = '◎';
        a.setAttribute('role', 'button');
        L.DomEvent.on(a, 'click', L.DomEvent.stopPropagation)
          .on(a, 'click', L.DomEvent.preventDefault)
          .on(a, 'click', () => {
            if (!navigator.geolocation) { alert('Konum desteklenmiyor.'); return; }
            a.style.opacity = '0.5';
            navigator.geolocation.getCurrentPosition(
              (pos) => {
                a.style.opacity = '1';
                const ll = [pos.coords.latitude, pos.coords.longitude] as L.LatLngTuple;
                map.setView(ll as L.LatLngExpression, 12, { animate: true });
                if (self.locateMarker) try { map.removeLayer(self.locateMarker); } catch (_) {}
                self.locateMarker = L.circleMarker(ll as L.LatLngExpression, {
                  radius: 9, fillColor: '#1a5276', color: '#fff', weight: 2, fillOpacity: 0.9,
                }).addTo(map).bindPopup('📍 Konumunuz').openPopup();
              },
              (err) => { a.style.opacity = '1'; alert('Konum alınamadı: ' + err.message); }
            );
          });
        return c;
      },
    });
    new LocateCtrl().addTo(this.map);
  }

  // ── Yardımcılar ──────────────────────────────────────────────────────────
  private removeLayer(layer: any): void {
    if (!layer) return;
    try { this.map.removeLayer(layer); } catch (_) {}
  }

  private removeDistrictLayer(): void {
    this.removeLayer(this.districtLayer);
    this.removeLayer(this.mahalleLayer);
    this.districtLayer = null;
    this.mahalleLayer = null;
    this.selectedDistrictName.set(null);
    this.districtCount.set(0);
    // İlçe ve outline kaldırılınca provincesLayer ve countriesLayer'ı haritaya geri ekle
    if (this.provincesLayer && !this.map?.hasLayer(this.provincesLayer)) {
      try { this.provincesLayer.addTo(this.map); } catch (_) {}
    }
    try { this.provincesLayer?.setStyle(S_PROV_DEF); } catch (_) {}
    if (this.countriesLayer && !this.map?.hasLayer(this.countriesLayer)) {
      try { this.countriesLayer.addTo(this.map); this.countriesLayer.bringToBack(); } catch (_) {}
    }
  }

  // ── OSM Overpass API ile gerçek mahalle verisi ────────────────────────────
  private loadMahallesFromOSM(districtFeature: any): void {
    if (!this.L || !this.map) return;
    this.removeLayer(this.mahalleLayer);
    this.mahalleLayer = null;

    const geo = districtFeature?.geometry;
    if (!geo?.coordinates) return;

    // İlçe geometrisinden bbox hesapla
    let minLng = Infinity, minLat = Infinity, maxLng = -Infinity, maxLat = -Infinity;
    const dig = (arr: any): void => {
      if (!Array.isArray(arr)) return;
      if (typeof arr[0] === 'number') {
        const [lng, lat] = arr as [number, number];
        if (lng < minLng) minLng = lng;
        if (lng > maxLng) maxLng = lng;
        if (lat < minLat) minLat = lat;
        if (lat > maxLat) maxLat = lat;
        return;
      }
      for (const item of arr) dig(item);
    };
    dig(geo.coordinates);
    if (!isFinite(minLng)) return;

    // Türkiye'de mahalleler OSM'de admin_level=10 olarak kayıtlıdır
    const query = `[out:json][timeout:25][bbox:${minLat},${minLng},${maxLat},${maxLng}];(relation["boundary"="administrative"]["admin_level"="10"];);out geom;`;
    const url = `https://overpass-api.de/api/interpreter?data=${encodeURIComponent(query)}`;

    this.loading.set(true);

    // Angular HttpClient+SSR/withFetch dış URL'leri engelleyebilir;
    // doğrudan tarayıcı fetch() kullanıyoruz, AbortController ile 20s timeout
    const ctrl = new AbortController();
    const tid = setTimeout(() => ctrl.abort(), 20000);

    window.fetch(url, { signal: ctrl.signal })
      .then(r => {
        clearTimeout(tid);
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json();
      })
      .then((data: any) => {
        this.loading.set(false);
        try {
          const features = this.overpassToGeoJSON(data);
          if (features.length > 0) this.renderMahalles(features);
        } catch (e) {
          console.warn('[Province] GeoJSON dönüşüm hatası', e);
        }
      })
      .catch((e: any) => {
        clearTimeout(tid);
        this.loading.set(false);
        if (e?.name !== 'AbortError') {
          console.warn('[Province] OSM mahalle verisi yüklenemedi', e);
        }
      });
  }

  // OSM Overpass JSON → GeoJSON Feature dizisi dönüşümü
  private overpassToGeoJSON(data: any): any[] {
    const elements: any[] = data?.elements ?? [];
    const features: any[] = [];

    for (const el of elements) {
      if (el.type !== 'relation') continue;
      const members: any[] = el.members ?? [];

      const outerWays: number[][][] = members
        .filter((m: any) => m.type === 'way' && m.role === 'outer')
        .map((m: any) => (m.geometry ?? []).map((pt: any) => [pt.lon, pt.lat]));

      if (!outerWays.length) continue;

      const ring = this.connectWays(outerWays);
      if (ring.length < 4) continue;

      const innerRings: number[][][] = members
        .filter((m: any) => m.type === 'way' && m.role === 'inner')
        .map((m: any) => (m.geometry ?? []).map((pt: any) => [pt.lon, pt.lat]))
        .filter((pts: number[][]) => pts.length >= 4);

      features.push({
        type: 'Feature',
        geometry: { type: 'Polygon', coordinates: [ring, ...innerRings] },
        properties: {
          name: el.tags?.name || el.tags?.['name:tr'] || 'Mahalle',
          osm_id: el.id
        }
      });
    }
    return features;
  }

  // Way parçalarını uç uca birleştirerek kapalı halka oluşturur
  private connectWays(ways: number[][][]): number[][] {
    if (!ways.length) return [];
    const result: number[][] = [...ways[0]];
    const remaining = ways.slice(1);

    while (remaining.length) {
      const last = result[result.length - 1];
      let found = false;
      for (let i = 0; i < remaining.length; i++) {
        const way = remaining[i];
        if (!way.length) { remaining.splice(i, 1); found = true; break; }
        const head = way[0];
        const tail = way[way.length - 1];
        if (Math.abs(head[0] - last[0]) < 1e-6 && Math.abs(head[1] - last[1]) < 1e-6) {
          result.push(...way.slice(1));
          remaining.splice(i, 1); found = true; break;
        } else if (Math.abs(tail[0] - last[0]) < 1e-6 && Math.abs(tail[1] - last[1]) < 1e-6) {
          result.push(...[...way].reverse().slice(1));
          remaining.splice(i, 1); found = true; break;
        }
      }
      if (!found) break;
    }

    // Halkayı kapat
    if (result.length > 1) {
      const f = result[0], l = result[result.length - 1];
      if (Math.abs(f[0] - l[0]) > 1e-6 || Math.abs(f[1] - l[1]) > 1e-6) result.push(result[0]);
    }
    return result;
  }

  // Mahalle GeoJSON feature'larını haritada çizer
  private renderMahalles(features: any[]): void {
    if (!this.L || !this.map) return;
    this.removeLayer(this.mahalleLayer);

    try {
      if (!this.map.getPane('mahallePane')) {
        this.map.createPane('mahallePane');
        const p = this.map.getPane('mahallePane') as HTMLElement;
        p.style.zIndex = '450';
        p.style.pointerEvents = 'none'; // İlçe tıklamalarını engelleme
      }
    } catch (_) {}

    const L = this.L;
    this.mahalleLayer = L.geoJSON(features, {
      pane: 'mahallePane',
      style: () => ({ color: '#7d3c98', weight: 1.4, fillColor: '#f5eef8', fillOpacity: 0.5 }),
      onEachFeature: (feat: any, layer: any) => {
        try {
          layer.bindTooltip(feat.properties?.name ?? 'Mahalle', {
            direction: 'center', className: 'map-tooltip'
          });
        } catch (_) {}
      }
    }).addTo(this.map);

    try { this.districtLayer?.bringToFront(); } catch (_) {}
  }

  private fitAll(): void {
    try {
      const b = this.provincesLayer?.getBounds();
      if (b?.isValid()) this.map.fitBounds(b, { padding: [20, 20], animate: true });
    } catch (_) {}
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    if (this.kbHandler) document.removeEventListener('keydown', this.kbHandler);
    try { this.map?.remove(); } catch (_) {}
    this.map = null;
  }

  goHome(): void { this.router.navigate(['/province']); }

  selectProvince(evt: any): void {
    const val = String(evt?.target?.value ?? '').trim();
    if (!val) { this.clearSelection(); return; }
    this.router.navigate(['/province', encodeURIComponent(val)]);
  }
}
