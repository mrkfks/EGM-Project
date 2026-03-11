import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, PLATFORM_ID, AfterViewInit, OnDestroy, OnInit } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import type * as Leaflet from 'leaflet';

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
  private resizeHandler = () => this.fitMapToScreen();

  constructor(private http: HttpClient, @Inject(PLATFORM_ID) private platformId: Object) {}

  async ngAfterViewInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;

    const L = await import('leaflet');
    this.L = L;

    L.Icon.Default.mergeOptions({
      iconUrl: 'assets/marker-icon.png',
      iconRetinaUrl: 'assets/marker-icon-2x.png',
      shadowUrl: 'assets/marker-shadow.png'
    });

    this.map = L.map('map', {
      center: [39, 35],
      zoom: 6,
      zoomControl: false,
      scrollWheelZoom: false,
      doubleClickZoom: false,
      touchZoom: false,
      dragging: false,
      boxZoom: false,
      keyboard: false
    });
    this.notifLayer = L.featureGroup().addTo(this.map);

    this.http.get('/assets/turkey-provinces-simplified.geojson').subscribe((geo: any) => {
      const defaultStyle = { color: '#1a3a5c', weight: 1.5, fillColor: '#4a90d9', fillOpacity: 0.4 };
      const geojsonLayer = L.geoJSON(geo, {
        style: () => ({ ...defaultStyle }),
        onEachFeature: (feature: any, layer: any) => {
          const name = feature?.properties?.Name
                    || feature?.properties?.name
                    || feature?.properties?.NAME_1
                    || 'İl';
          layer.bindTooltip(name, { sticky: true });
          layer.on({
            mouseover: (e: any) => {
              e.target.setStyle({ fillColor: '#e67e22', fillOpacity: 0.7 });
              e.target.bringToFront();
            },
            mouseout: (e: any) => {
              geojsonLayer.resetStyle(e.target);
            },
            click: (e: any) => {
              L.popup()
                .setLatLng(e.latlng)
                .setContent(`<b>${name}</b>`)
                .openOn(this.map!);
              this.onProvinceClick(name, feature);
            }
          });
        }
      }).addTo(this.map!);

      // Haritayı Türkiye'ye sığdır (çoklu deneme — layout oturmasını bekler)
      this.geoBounds = geojsonLayer.getBounds();
      const fitMap = () => this.fitMapToScreen();
      fitMap();
      requestAnimationFrame(() => requestAnimationFrame(fitMap));
      setTimeout(fitMap, 300);

      // Pencere boyutu değiştiğinde haritayı yeniden ölçekle
      window.addEventListener('resize', this.resizeHandler);
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
      window.removeEventListener('resize', this.resizeHandler);
    }
    if (this.map) this.map.remove();
  }

  private fitMapToScreen(): void {
    if (!this.map || !this.geoBounds) return;
    this.map.invalidateSize();
    this.map.fitBounds(this.geoBounds, { padding: [20, 20], animate: false });
  }

  onProvinceClick(name: string, feature: any) {
    const sample = [{ lat: 39.9, lng: 32.8, text: `${name} — örnek bildirim` }];
    this.showNotifications(sample);
  }

  showNotifications(items: {lat:number,lng:number,text:string}[]) {
    if (!this.notifLayer || !this.map || !this.L) return;
    const L = this.L;
    this.notifLayer.clearLayers();
    items.forEach(i => {
      L.circleMarker([i.lat, i.lng], { radius: 6, color: 'red', fillOpacity: 0.9 }).bindPopup(i.text).addTo(this.notifLayer!);
    });
    if (items.length) this.map.fitBounds(this.notifLayer.getBounds(), { maxZoom: 10 });
  }

  
}