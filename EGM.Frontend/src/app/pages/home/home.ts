import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, PLATFORM_ID, AfterViewInit, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import type * as Leaflet from 'leaflet';

// Türkiye sınırları (SW, NE) — maxBounds olarak kullanılır
const TURKEY_SW: [number, number] = [35.5, 25.5];
const TURKEY_NE: [number, number] = [42.5, 45.0];

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './home.html',
  host: { style: 'display:block; position:absolute; inset:0;' }
})
export class Home implements AfterViewInit, OnDestroy, OnInit {
  private map?: Leaflet.Map;
  private notifLayer?: Leaflet.FeatureGroup;
  private L?: typeof Leaflet;
  private geoBounds?: Leaflet.LatLngBounds;
  private resizeObserver?: ResizeObserver;
  private resizeTimer?: number;

  constructor(private http: HttpClient, private router: Router, @Inject(PLATFORM_ID) private platformId: Object) {}

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;

    const L = await import('leaflet');
    this.L = L;

    L.Icon.Default.mergeOptions({
      iconUrl: 'assets/marker-icon.png',
      iconRetinaUrl: 'assets/marker-icon-2x.png',
      shadowUrl: 'assets/marker-shadow.png'
    });

    // maxBounds: Türkiye + komşuları kadar geniş bir alan; dışına çıkılamaz
    const maxBounds = L.latLngBounds(
      L.latLng(TURKEY_SW[0] - 4, TURKEY_SW[1] - 6),
      L.latLng(TURKEY_NE[0] + 4, TURKEY_NE[1] + 6)
    );

    this.map = L.map('map', {
      center: [39, 35],
      zoom: 6,
      minZoom: 5,
      maxZoom: 10,
      maxBounds,
      maxBoundsViscosity: 1.0,
      zoomControl: false,
      scrollWheelZoom: false,
      doubleClickZoom: false,
      touchZoom: false,
      dragging: false,
      boxZoom: false,
      keyboard: false
    });
    this.notifLayer = L.featureGroup().addTo(this.map);

    // ResizeObserver: container boyutu değişince invalidateSize + fitMap
    const mapContainer = this.map.getContainer();
    this.resizeObserver = new ResizeObserver(() => {
      if (this.resizeTimer) window.clearTimeout(this.resizeTimer);
      this.resizeTimer = window.setTimeout(() => {
        if (this.map) {
          this.map.invalidateSize();
          this.fitMapToScreen();
        }
      }, 100);
    });
    this.resizeObserver.observe(mapContainer);

    const smallScreen = (typeof window !== 'undefined') && window.innerWidth <= 768;
    const geoDefault = '/assets/turkey-provinces-simplified.geojson';
    const geoSmall = '/assets/turkey-provinces-simplified-low.geojson';
    const geoPath = smallScreen ? geoSmall : geoDefault;

    // Fallback: küçük ekran dosyası yoksa varsayılanı yükle
    const fallbackGet = (path: string, fallback: string, cb: (geo: any) => void) => {
      this.http.get(path).subscribe(cb, () => {
        if (fallback && fallback !== path) this.http.get(fallback).subscribe(cb, () => console.warn('GeoJSON load failed'));
      });
    };

    fallbackGet(geoPath, geoDefault, (geo: any) => {
      const defaultStyle = { color: '#0b3954', weight: smallScreen ? 0.9 : 1.2, fillColor: '#f2f6fb', fillOpacity: smallScreen ? 0.9 : 0.95 };
      const geojsonLayer = L.geoJSON(geo, {
        style: () => ({ ...defaultStyle }),
        onEachFeature: (feature: any, layer: any) => {
          const name = feature?.properties?.Name
                    || feature?.properties?.name
                    || feature?.properties?.NAME_1
                    || 'İl';
          layer.bindTooltip(name, { sticky: true });
          layer.on({
            mouseover: () => {
              const path = (layer as any)._path;
              if (path && path.classList) path.classList.add('hovered');
            },
            mouseout: () => {
              const path = (layer as any)._path;
              if (path && path.classList) path.classList.remove('hovered');
              geojsonLayer.resetStyle(layer);
            },
            click: (e: any) => {
              // Open province page without zooming or changing map view
              this.onProvinceClick(name, feature);
            }
          });
        }
      }).addTo(this.map!);

      // Provinces are already added to the map above via .addTo(this.map)

      // Haritayı Türkiye'ye sığdır (çoklu deneme — layout oturmasını bekler)
      this.geoBounds = geojsonLayer.getBounds();
      const fitMap = () => this.fitMapToScreen();
      fitMap();
      requestAnimationFrame(() => requestAnimationFrame(fitMap));
      setTimeout(fitMap, 300);

      // Küçük ekranlarda touch etkileşimleri aç
      if (smallScreen && this.map) {
        try { this.map.dragging.enable(); } catch (_) {}
        try { this.map.touchZoom.enable(); } catch (_) {}
      }

      // Fetch countries dataset, pick features that intersect an expanded Turkey bbox
      // and add them as a 'Country Borders' overlay (shows GR, BG, etc.).
      this.http.get('https://raw.githubusercontent.com/datasets/geo-countries/master/data/countries.geojson')
        .subscribe((countries: any) => {
          try {
            const features = countries?.features || [];
            if (!this.geoBounds) return;
            // Haritada görünebilecek tüm ülkeleri kapsayan sabit bbox
            // (Akdeniz, Karadeniz, Orta Doğu, Kuzey Afrika dahil)
            const expanded = L.latLngBounds(
              L.latLng(10, -5),   // SW: Kuzey Afrika'nın altı, Batı Akdeniz
              L.latLng(58, 70)    // NE: Ukrayna/Rusya'nın kuzeyi, İran'ın doğusu
            );
            const neighbors: any[] = [];
            for (const f of features) {
              try {
                const fbounds = L.geoJSON(f).getBounds();
                if (fbounds && fbounds.isValid() && fbounds.intersects(expanded)) {
                  neighbors.push(f);
                }
              } catch (_) {
                // skip malformed feature
              }
            }
            if (neighbors.length) {
              // Generate a stable, unique color per neighbor using a hash -> HSL mapping
              const hashString = (s: string) => {
                let h = 0;
                for (let i = 0; i < s.length; i++) {
                  h = ((h << 5) - h) + s.charCodeAt(i);
                  h |= 0;
                }
                return h;
              };
              const neighborsLayer = L.geoJSON(neighbors, {
                style: (feature: any) => {
                  const key = (feature && (feature.properties?.ISO_A3 || feature.properties?.ADMIN || feature.properties?.name || '')) + '';
                  const hash = Math.abs(hashString(key || (feature && feature.id) || Math.random().toString()));
                  const hue = hash % 360;
                  const fillColor = `hsl(${hue}, 60%, ${smallScreen ? 90 : 85}%)`;
                  return { color: '#0b3954', weight: smallScreen ? 1.6 : 3.5, fill: true, fillColor, fillOpacity: smallScreen ? 0.85 : 0.95 };
                }
              }).addTo(this.map!);
              // ensure provinces (geojsonLayer) render above neighboring countries
              try { geojsonLayer.bringToFront(); } catch (e) {}

              // Haritayı Türkiye'ye sığdır (komşular yüklendikten sonra tekrar)
              this.fitMapToScreen();
            }
          } catch (err) {
            console.warn('Country boundaries load failed', err);
          }
        }, () => { /* ignore network errors */ });
    });
  }

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      document.body.classList.add('no-scroll');
    }
  }

  ngOnDestroy(): void {
    if (isPlatformBrowser(this.platformId)) {
      document.body.classList.remove('no-scroll');
    }
    if (this.resizeObserver) { this.resizeObserver.disconnect(); this.resizeObserver = undefined; }
    if (this.resizeTimer) { window.clearTimeout(this.resizeTimer); }
    if (this.map) this.map.remove();
  }

  private fitMapToScreen(): void {
    if (!this.map || !this.geoBounds) return;
    this.map.invalidateSize();

    // Haritayı kapsayıcısına tam sığdır — resim gibi, fazla boşluk bırakma
    this.map.fitBounds(this.geoBounds, {
      padding: [10, 10],
      animate: false
    });
  }

  onProvinceClick(name: string, feature: any) {
    const id = feature?.properties?.id || feature?.properties?.NAME_1 || feature?.properties?.name || name;
    const safe = encodeURIComponent(String(id));
    this.router.navigate(['/province', safe]);
  }

  showNotifications(items: {lat:number,lng:number,text:string}[]) {
    if (!this.notifLayer || !this.map || !this.L) return;
    const L = this.L;
    this.notifLayer.clearLayers();
    items.forEach(i => {
      L.circleMarker([i.lat, i.lng], { radius: 6, color: 'red', fillOpacity: 0.9 }).bindPopup(i.text).addTo(this.notifLayer!);
    });
    if (items.length) {
      const size = this.map.getSize();
      const pad = Math.max(20, Math.round(Math.min(size.x, size.y) * 0.06));
      const sidebar = document.querySelector('.egm-sidebar') as HTMLElement | null;
      const leftOffset = sidebar ? Math.round(sidebar.getBoundingClientRect().width) : 0;
      this.map.fitBounds(this.notifLayer.getBounds(), { maxZoom: 10, paddingTopLeft: [leftOffset + pad, pad], paddingBottomRight: [pad, pad] });
    }
  }

  
}