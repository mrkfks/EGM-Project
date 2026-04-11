import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GeoService, GeoJsonFeatureCollection, Province } from '../../services/geo.service';
import * as L from 'leaflet';

@Component({
  selector: 'app-harita',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './harita.component.html',
  styleUrls: ['./harita.component.css']
})
export class HaritaComponent implements OnInit {
  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef<HTMLDivElement>;

  private map!: L.Map;
  private olayMarkersGroup: L.FeatureGroup = L.featureGroup();
  private vipMarkersGroup: L.FeatureGroup = L.featureGroup();
  private provinceBoundariesGroup: L.FeatureGroup = L.featureGroup();

  // İl/İlçe seçimleri
  provinces: Province[] = [];
  selectedProvince: string = '';
  selectedYear: number = new Date().getFullYear();

  // Toggle seçenekleri
  showOlaylar: boolean = true;
  showVIPZiyaretler: boolean = true;
  showProvinceBoundaries: boolean = true;

  // Yükleme durumu
  isLoading: boolean = false;
  errorMessage: string = '';

  // Türkiye merkez koordinatları
  turkeyCenter: L.LatLngExpression = [38.9637, 35.2433];
  initialZoom: number = 6;

  constructor(private geoService: GeoService) {}

  ngOnInit(): void {
    setTimeout(() => {
      this.initializeMap();
      this.loadProvinces();
      this.loadMapData();
    }, 100);
  }

  /**
   * Haritayı başlat
   */
  private initializeMap(): void {
    if (!this.mapContainer) {
      console.error('Harita konteyner bulunamadı');
      return;
    }

    this.map = L.map(this.mapContainer.nativeElement).setView(this.turkeyCenter, this.initialZoom);

    // OpenStreetMap tile layer (OSM)
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap kontributörleri',
      maxZoom: 20,
      minZoom: 5
    }).addTo(this.map);

    // Katmanları haritaya ekle
    this.olayMarkersGroup.addTo(this.map);
    this.vipMarkersGroup.addTo(this.map);
    this.provinceBoundariesGroup.addTo(this.map);

    // Harita çalıştırıldığında tüm kontrol tuşlarını aç
    this.setupMapControls();
  }

  /**
   * Harita kontrol tuşlarını ayarla (zoom, vb.)
   */
  private setupMapControls(): void {
    // Zaten default olarak var ama burada ekstra kontrol olabilir
  }

  /**
   * İlleri yükle
   */
  private loadProvinces(): void {
    this.geoService.getProvinces().subscribe(
      (data) => {
        this.provinces = data.sort((a, b) => a.name.localeCompare(b.name));
      },
      (error) => {
        console.error('İlleri yükleme hatası:', error);
        this.errorMessage = 'İller yüklenemedi';
      }
    );
  }

  /**
   * Harita verilerini yükle (olaylar, VIP ziyaretler)
   */
  loadMapData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    // Olayları yükle
    if (this.showOlaylar) {
      this.geoService.getOlaylarForMap(
        this.selectedProvince || undefined,
        undefined,
        this.selectedYear
      ).subscribe(
        (data) => {
          this.displayOlaylar(data);
        },
        (error) => {
          console.error('Olayları yükleme hatası:', error);
          this.errorMessage = 'Olaylar yüklenemedi';
        }
      );
    }

    // VIP ziyaretlerini yükle
    if (this.showVIPZiyaretler) {
      this.geoService.getVIPZiyaretlerForMap(
        this.selectedProvince || undefined
      ).subscribe(
        (data) => {
          this.displayVIPZiyaretler(data);
        },
        (error) => {
          console.error('VIP ziyaretlerini yükleme hatası:', error);
          this.errorMessage = 'VIP ziyaretleri yüklenemedi';
        }
      );
    }

    // İl sınırlarını yükle
    if (this.showProvinceBoundaries) {
      this.loadProvinceBoundaries();
    }

    this.isLoading = false;
  }

  /**
   * Olayları haritaya göster
   */
  private displayOlaylar(data: GeoJsonFeatureCollection): void {
    this.olayMarkersGroup.clearLayers();

    data.features.forEach((feature) => {
      const { geometry, properties } = feature;

      if (geometry.type === 'Point') {
        const [lng, lat] = geometry.coordinates;

        // Kırmızı işaretçi
        const marker = L.circleMarker([lat, lng], {
          radius: 6,
          fillColor: '#ef4444',
          color: '#dc2626',
          weight: 2,
          opacity: 0.8,
          fillOpacity: 0.7
        });

        // Popup
        const popupContent = `
          <div class="popup-content">
            <strong>${properties['takipNo'] || 'TakipNo'}</strong><br/>
            <small>${properties['il']} / ${properties['ilce']}</small><br/>
            <small>${properties['mekan']}</small><br/>
            <small>${new Date(properties['tarih']).toLocaleDateString('tr-TR')}</small><br/>
            <small>Durum: ${properties['durum']}</small>
          </div>
        `;

        marker.bindPopup(popupContent);
        this.olayMarkersGroup.addLayer(marker);
      }
    });

    console.log(`${data.features.length} olay yüklendi`);
  }

  /**
   * VIP ziyaretlerini haritaya göster
   */
  private displayVIPZiyaretler(data: GeoJsonFeatureCollection): void {
    this.vipMarkersGroup.clearLayers();

    data.features.forEach((feature) => {
      const { geometry, properties } = feature;

      if (geometry.type === 'Point') {
        const [lng, lat] = geometry.coordinates;

        // Mavi işaretçi
        const marker = L.circleMarker([lat, lng], {
          radius: 6,
          fillColor: '#3b82f6',
          color: '#1e40af',
          weight: 2,
          opacity: 0.8,
          fillOpacity: 0.7
        });

        // Popup
        const popupContent = `
          <div class="popup-content">
            <strong>${properties['ziyaretEdenAdSoyad'] || properties['takipNo']}</strong><br/>
            <small>${properties['unvan']}</small><br/>
            <small>${properties['il']}</small><br/>
            <small>${properties['mekan']}</small><br/>
            <small>Başlangıç: ${new Date(properties['baslangicTarihi']).toLocaleDateString('tr-TR')}</small><br/>
            <small>Bitiş: ${new Date(properties['bitisTarihi']).toLocaleDateString('tr-TR')}</small>
          </div>
        `;

        marker.bindPopup(popupContent);
        this.vipMarkersGroup.addLayer(marker);
      }
    });

    console.log(`${data.features.length} VIP ziyareti yüklendi`);
  }

  /**
   * İl sınırlarını haritaya göster
   */
  private loadProvinceBoundaries(): void {
    this.provinceBoundariesGroup.clearLayers();

    this.provinces.forEach((province) => {
      this.geoService.getGeometry(province.osmId).subscribe(
        (data) => {
          const geoJsonLayer = L.geoJSON(data, {
            style: {
              color: '#666',
              weight: 1,
              opacity: 0.5,
              fill: false
            }
          });
          this.provinceBoundariesGroup.addLayer(geoJsonLayer);
        },
        (error) => {
          console.log(`${province.name} sınırı yüklenemedi`);
        }
      );
    });
  }

  /**
   * İl seçimi değiştiğinde
   */
  onProvinceChange(): void {
    this.loadMapData();
  }

  /**
   * Yıl seçimi değiştiğinde
   */
  onYearChange(): void {
    this.loadMapData();
  }

  /**
   * Toggle seçenekleri
   */
  toggleOlaylar(): void {
    this.showOlaylar = !this.showOlaylar;
    if (this.showOlaylar) {
      this.olayMarkersGroup.addTo(this.map);
      this.geoService.getOlaylarForMap(
        this.selectedProvince || undefined,
        undefined,
        this.selectedYear
      ).subscribe((data) => this.displayOlaylar(data));
    } else {
      this.olayMarkersGroup.removeFrom(this.map);
    }
  }

  toggleVIPZiyaretler(): void {
    this.showVIPZiyaretler = !this.showVIPZiyaretler;
    if (this.showVIPZiyaretler) {
      this.vipMarkersGroup.addTo(this.map);
      this.geoService.getVIPZiyaretlerForMap(
        this.selectedProvince || undefined
      ).subscribe((data) => this.displayVIPZiyaretler(data));
    } else {
      this.vipMarkersGroup.removeFrom(this.map);
    }
  }

  toggleProvinceBoundaries(): void {
    this.showProvinceBoundaries = !this.showProvinceBoundaries;
    if (this.showProvinceBoundaries) {
      this.provinceBoundariesGroup.addTo(this.map);
      this.loadProvinceBoundaries();
    } else {
      this.provinceBoundariesGroup.removeFrom(this.map);
    }
  }

  /**
   * Haritayı sıfırla (tüm iller görüntülensin)
   */
  resetMap(): void {
    this.selectedProvince = '';
    this.map.setView(this.turkeyCenter, this.initialZoom);
    this.loadMapData();
  }
}
