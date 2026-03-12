import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-province',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div style="display:flex;flex-direction:column;height:100vh;">
    <!-- Floating back button placed over the map next to the sidebar (adjust left value if needed) -->
    <div style="position:absolute;top:56px;left:calc(190px + 12px);z-index:3000;display:flex;gap:8px;align-items:center;pointer-events:auto;">
      <button (click)="goHome()" style="background:#fff;border:1px solid #333;padding:6px 10px;border-radius:4px;box-shadow:0 1px 3px rgba(0,0,0,.12);pointer-events:auto;">Türkiye Haritasına Dön</button>
      <select (change)="selectProvince($event)" style="padding:6px;border-radius:4px;border:1px solid #ccc;background:#fff;pointer-events:auto;" tabindex="0">
        <option value="">İl seçin...</option>
        <option *ngFor="let p of provinces" [value]="p.id" [selected]="p.id===selectedProvinceId">{{p.name}}</option>
      </select>
    </div>
    <div id="provinceMap" style="flex:1;min-height:200px;"></div>
  </div>
  `,
  host: { style: 'display:block; position:relative; inset:0;' }
})
export class Province implements OnInit {
  provinceName = '';
  provinces: Array<{ id: string, name: string }> = [];
  selectedProvinceId: string | null = null;
  private map: any;

  constructor(private route: ActivatedRoute, private router: Router, @Inject(PLATFORM_ID) private platformId: Object) {}

  async ngOnInit(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) return;
    const rawId = this.route.snapshot.paramMap.get('id') || '';
    const id = decodeURIComponent(rawId);
    this.provinceName = id;

    const L = await import('leaflet');
    (L as any).Icon.Default.mergeOptions({
      iconUrl: 'assets/marker-icon.png',
      iconRetinaUrl: 'assets/marker-icon-2x.png',
      shadowUrl: 'assets/marker-shadow.png'
    });

    this.map = L.map('provinceMap', { center: [39, 35], zoom: 6, attributionControl: false, zoomControl: false, scrollWheelZoom: false, doubleClickZoom: false, touchZoom: false, dragging: false, boxZoom: false, keyboard: false });
    // No external basemap — render as vector-only map and apply a local background color
    try {
      const container = this.map.getContainer();
      if (container && (container as HTMLElement).style) {
        (container as HTMLElement).style.background = '#f2f6fb';
      }
    } catch (e) { /* ignore */ }

    fetch('/assets/turkey-provinces-simplified.geojson')
      .then(r => r.json())
      .then((geo: any) => {
      const features = geo?.features || [];
      // build province list for the selector
      this.provinces = features
        .map((f: any) => {
          const p = f.properties || {};
          const nameVal = p.Name || p.name || p.NAME_1 || '';
          if (!nameVal) return null;
          return { id: String(nameVal), name: String(nameVal) };
        })
        .filter((x: any): x is { id: string; name: string } => !!x)
        .sort((a: { name: string }, b: { name: string }) => a.name.localeCompare(b.name, 'tr'));

      // Add all provinces as a light background layer so they are visible
      const defaultStyle = { color: '#0b3954', weight: 1, fillColor: '#f2f6fb', fillOpacity: 0.9 };
      const allLayer = L.geoJSON(features, {
        style: () => ({ ...defaultStyle }),
        onEachFeature: (feature: any, layer: any) => {
          const name = feature?.properties?.Name || feature?.properties?.name || feature?.properties?.NAME_1 || 'İl';
          layer.bindTooltip(name, { direction: 'center', permanent: false, className: 'small-tooltip' });
        }
      }).addTo(this.map);

      // Try to find the requested province id (if provided) and highlight it
      const match = features.find((f: any) => {
        const p = f.properties || {};
        return p.Name === id || p.name === id || p.NAME_1 === id || String(p.id) === id;
      }) || features.find((f: any) => {
        const p = f.properties || {};
        return (p.Name && p.Name.toLowerCase() === id.toLowerCase()) || (p.name && p.name.toLowerCase() === id.toLowerCase()) || (p.NAME_1 && p.NAME_1.toLowerCase() === id.toLowerCase());
      });

      if (match) {
        const highlight = L.geoJSON(match, { style: { color: '#e67e22', weight: 2.5, fillOpacity: 0.85 } }).addTo(this.map);
        const bounds = highlight.getBounds();
        if (bounds && bounds.isValid && bounds.isValid()) {
          this.map.fitBounds(bounds, { padding: [10, 10], animate: false });
          // Lock zoom level and interactions so the map is a fixed static view
          try {
            const currentZoom = this.map.getZoom();
            this.map.options.minZoom = currentZoom;
            this.map.options.maxZoom = currentZoom;
          } catch (e) {}
          try { this.map.dragging && this.map.dragging.disable(); } catch (e) {}
          try { this.map.touchZoom && this.map.touchZoom.disable(); } catch (e) {}
          try { this.map.scrollWheelZoom && this.map.scrollWheelZoom.disable(); } catch (e) {}
          try { this.map.doubleClickZoom && this.map.doubleClickZoom.disable(); } catch (e) {}
          try { this.map.boxZoom && this.map.boxZoom.disable(); } catch (e) {}
          try { this.map.keyboard && this.map.keyboard.disable(); } catch (e) {}
        }
        // Ensure any automatically-added zoom control is removed
        try { if (this.map && this.map.zoomControl) { this.map.removeControl(this.map.zoomControl); } } catch (e) {}
        this.provinceName = match.properties?.Name || match.properties?.name || match.properties?.NAME_1 || id;
        this.selectedProvinceId = String(id);
      } else {
        // No specific province requested — fit to full extent
        try {
          const bounds = allLayer.getBounds();
          if (bounds && bounds.isValid && bounds.isValid()) this.map.fitBounds(bounds, { padding: [10,10], animate: false });
        } catch (e) {}
      }
    }).catch((err: any) => console.warn('Failed to load province geojson', err));
  }

  goHome() { this.router.navigate(['/home']); }

  selectProvince(evt: any) {
    const val = evt?.target?.value || evt;
    if (!val) return;
    const safe = encodeURIComponent(String(val));
    this.router.navigate(['/province', safe]);
  }
}
