import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { GeoService, Province, Coordinates } from '../../services/geo.service';
import { OlayService } from '../../services/olay.service';
import { OlayFilterRequest, OlayData, OlayDurumEnum } from '../../models/olay-filter.model';
import { HaritaControlPanelComponent } from './harita-control-panel.component';
import * as L from 'leaflet';

@Component({
  selector: 'app-harita',
  standalone: true,
  imports: [CommonModule, FormsModule, HaritaControlPanelComponent],
  templateUrl: './harita.component.html',
  styleUrls: ['./harita.component.css']
})
export class HaritaComponent implements OnInit, OnDestroy {
  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef<HTMLDivElement>;

  private map!: L.Map;
  private olayMarkersGroup: L.FeatureGroup = L.featureGroup();
  private vipMarkersGroup: L.FeatureGroup = L.featureGroup();
  private provinceBoundariesGroup: L.FeatureGroup = L.featureGroup();

  // Filtre durumu
  provinces: Province[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';
  selectedProvince: string = '';
  showVIPZiyaretler: boolean = false;
  showProvinceBoundaries: boolean = false;

  // Türkiye merkez koordinatları
  turkeyCenter: L.LatLngExpression = [38.9637, 35.2433];
  initialZoom: number = 6;

  // İl merkez koordinatlarının fallback map'i (API başarısız olursa kullan)
  private ilMerkezleri: { [key: string]: Coordinates } = {
    'Adana': { latitude: 36.9909, longitude: 35.3213 },
    'Adıyaman': { latitude: 37.7648, longitude: 38.2788 },
    'Ankara': { latitude: 39.9334, longitude: 32.8597 },
    'Artvin': { latitude: 41.1828, longitude: 41.8218 },
    'Aydın': { latitude: 37.8446, longitude: 27.8409 },
    'Balıkesir': { latitude: 39.6485, longitude: 27.8826 },
    'Bilecik': { latitude: 40.1425, longitude: 29.9796 },
    'Bingöl': { latitude: 38.8851, longitude: 40.4967 },
    'Bitlis': { latitude: 38.3938, longitude: 42.1073 },
    'Bolu': { latitude: 40.7334, longitude: 31.5678 },
    'Burdur': { latitude: 37.7203, longitude: 29.9830 },
    'Bursa': { latitude: 40.1955, longitude: 29.0667 },
    'Çanakkale': { latitude: 40.1489, longitude: 26.4133 },
    'Çankırı': { latitude: 40.6029, longitude: 33.6167 },
    'Çorum': { latitude: 40.5399, longitude: 34.9502 },
    'Denizli': { latitude: 37.7852, longitude: 29.0837 },
    'Diyarbakır': { latitude: 37.9144, longitude: 40.2306 },
    'Düzce': { latitude: 40.8438, longitude: 31.3618 },
    'Edirne': { latitude: 41.1673, longitude: 26.5553 },
    'Elazığ': { latitude: 38.6810, longitude: 39.2272 },
    'Erzincan': { latitude: 39.7465, longitude: 39.5006 },
    'Erzurum': { latitude: 39.9139, longitude: 41.2717 },
    'Eskişehir': { latitude: 39.7667, longitude: 30.5256 },
    'Gaziantep': { latitude: 37.0662, longitude: 37.3833 },
    'Giresun': { latitude: 40.9128, longitude: 38.6267 },
    'Gümüşhane': { latitude: 40.4611, longitude: 39.4811 },
    'Hakkari': { latitude: 37.5758, longitude: 43.7383 },
    'Hatay': { latitude: 36.4018, longitude: 36.3408 },
    'Iğdır': { latitude: 38.2760, longitude: 44.0608 },
    'Isparta': { latitude: 37.7648, longitude: 30.5566 },
    'İçel': { latitude: 36.7315, longitude: 34.6416 },
    'İstanbul': { latitude: 41.0082, longitude: 28.9784 },
    'İzmir': { latitude: 38.4161, longitude: 27.1398 },
    'Kahramanmaraş': { latitude: 37.5856, longitude: 36.9371 },
    'Karabük': { latitude: 41.1956, longitude: 32.6157 },
    'Karaman': { latitude: 37.1829, longitude: 33.2287 },
    'Kars': { latitude: 40.6167, longitude: 43.0975 },
    'Kastamonu': { latitude: 41.3887, longitude: 33.7827 },
    'Kayseri': { latitude: 38.7312, longitude: 35.4787 },
    'Kırıkkale': { latitude: 39.8460, longitude: 33.5153 },
    'Kırklareli': { latitude: 41.7371, longitude: 27.2259 },
    'Kırşehir': { latitude: 39.1450, longitude: 34.1656 },
    'Kilis': { latitude: 36.7184, longitude: 37.1338 },
    'Kocaeli': { latitude: 40.8634, longitude: 29.9464 },
    'Konya': { latitude: 37.8744, longitude: 32.4744 },
    'Kütahya': { latitude: 39.4167, longitude: 29.9833 },
    'Laâp': { latitude: 39.2404, longitude: 26.0886 },
    'Malatya': { latitude: 38.3552, longitude: 38.3091 },
    'Manisa': { latitude: 38.6349, longitude: 27.4787 },
    'Mardin': { latitude: 37.3089, longitude: 40.7347 },
    'Mersin': { latitude: 36.7315, longitude: 34.6416 },
    'Muğla': { latitude: 37.2158, longitude: 28.3644 },
    'Muş': { latitude: 38.7465, longitude: 41.4867 },
    'Nevşehir': { latitude: 38.6939, longitude: 34.7257 },
    'Niğde': { latitude: 37.9667, longitude: 34.6833 },
    'Ordu': { latitude: 40.9847, longitude: 37.2797 },
    'Rize': { latitude: 41.2206, longitude: 40.5103 },
    'Sakarya': { latitude: 40.7595, longitude: 30.3876 },
    'Samsun': { latitude: 41.2867, longitude: 35.6428 },
    'Siirt': { latitude: 37.9272, longitude: 41.9506 },
    'Sinop': { latitude: 42.0211, longitude: 35.1536 },
    'Sivas': { latitude: 39.7464, longitude: 36.4897 },
    'Tekirdağ': { latitude: 40.9831, longitude: 27.4711 },
    'Tokat': { latitude: 40.3163, longitude: 36.5557 },
    'Trabzon': { latitude: 40.7669, longitude: 39.7674 },
    'Tunceli': { latitude: 39.1081, longitude: 39.5502 },
    'Uşak': { latitude: 38.6823, longitude: 29.4084 },
    'Van': { latitude: 38.4161, longitude: 43.4142 },
    'Yalova': { latitude: 40.6500, longitude: 29.2667 },
    'Yozgat': { latitude: 39.8179, longitude: 35.8039 },
    'Zonguldak': { latitude: 41.4564, longitude: 31.7992 }
  };

  constructor(
    private geoService: GeoService,
    private olayService: OlayService
  ) {}

  ngOnInit(): void {
    setTimeout(() => {
      this.initializeMap();
      this.loadProvinces();
      this.applyDefaultFilter();

      // Pencere boyutu değiştiğinde haritayı güncelle
      window.addEventListener('resize', () => {
        if (this.map) {
          this.map.invalidateSize();
        }
      });
    }, 100);
  }

  ngAfterViewInit(): void {
    // Legend'i navbar ortasına HTML olarak enjekte et
    setTimeout(() => {
      this.injectLegendToNavbar();
    }, 300);
  }

  ngOnDestroy(): void {
    // Legend'i navbar'dan kaldır
    const navbarCenter = document.getElementById('navbar-center-outlet');
    if (navbarCenter) {
      navbarCenter.innerHTML = '';
    }
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
  }

  /**
   * İlleri yükle (dropdown için)
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
   * Filtre uygulandığında
   */
  onFilterApplied(filter: OlayFilterRequest): void {
    this.isLoading = true;
    this.errorMessage = '';

    // Sayfa bilgisi ekle
    filter.page = 1;
    filter.pageSize = 100;

    this.olayService.getFilteredForMap(filter).subscribe(
      (response) => {
        this.displayOlaylar(response.items);
        this.isLoading = false;
      },
      (error) => {
        console.error('Olayları yükleme hatası:', error);
        this.errorMessage = 'Olaylar yüklenemedi';
        this.isLoading = false;
        this.olayMarkersGroup.clearLayers();
      }
    );
  }

  /**
   * Varsayılan filteri uygula (tüm alanlar boş)
   */
  private applyDefaultFilter(): void {
    const defaultFilter: OlayFilterRequest = {
      page: 1,
      pageSize: 100
    };

    this.onFilterApplied(defaultFilter);
  }

  /**
   * Olayları haritaya göster.
   * Koordinat eksikse il merkezi koordinatını fallback olarak kullanır.
   */
  private displayOlaylar(olaylar: OlayData[]): void {
    this.olayMarkersGroup.clearLayers();

    // Koordinatsız olaylar için il merkezi yükle
    const olaylarKoordinatsiz = olaylar.filter(o => o.latitude == null || o.longitude == null);
    const benzersizIller = [...new Set(olaylarKoordinatsiz.map(o => o.il).filter((il): il is string => !!il))];

    // İlmerkez map'ini hazırla (fallback koordinatlarla başla)
    const ilKoordinatMap = new Map<string, Coordinates>();
    benzersizIller.forEach(il => {
      if (this.ilMerkezleri[il]) {
        ilKoordinatMap.set(il, this.ilMerkezleri[il]);
      }
    });

    if (benzersizIller.length > 0) {
      const requests = benzersizIller.map(il =>
        this.geoService.getCoordinates(il).pipe(
          map(coords => ({ il, coords })),
          catchError(() => of({ il, coords: null as Coordinates | null }))
        )
      );

      forkJoin(requests).subscribe(sonuclar => {
        // API'den dönen koordinatlarla fallback'i güncelle
        sonuclar.forEach(({ il, coords }) => { 
          if (coords) {
            ilKoordinatMap.set(il, coords);
          }
        });
        this.renderMarkers(olaylar, ilKoordinatMap);
      });
    } else {
      this.renderMarkers(olaylar, ilKoordinatMap);
    }
  }

  /**
   * Olayları marker olarak haritaya çizer.
   */
  private renderMarkers(olaylar: OlayData[], ilKoordinatMap: Map<string, Coordinates>): void {
    this.olayMarkersGroup.clearLayers();
    let gosterilenSayi = 0;

    olaylar.forEach((olay) => {
      let lat = olay.latitude ?? null;
      let lng = olay.longitude ?? null;
      let kesinKonum = lat != null && lng != null;

      // Koordinat yoksa il merkezini kullan
      if ((lat == null || lng == null) && olay.il) {
        const ilCoords = ilKoordinatMap.get(olay.il);
        if (ilCoords) {
          lat = ilCoords.latitude;
          lng = ilCoords.longitude;
          kesinKonum = false;
        }
      }

      if (lat == null || lng == null) return;

      // Gerçek durumu belirle (tarih bilgilerine göre)
      const gercekDurum = this.getGercekDurum(olay);

      // Duruma göre renk belirle
      const color = this.getColorByDurum(gercekDurum);

      // Kesin konumda dolu daire, il merkezinde bordo kenarlı + küçük
      const marker = L.circleMarker([lat, lng], {
        radius: kesinKonum ? 7 : 5,
        fillColor: color.fill,
        color: kesinKonum ? color.border : '#374151',
        weight: kesinKonum ? 2 : 1,
        opacity: 0.9,
        fillOpacity: kesinKonum ? 0.8 : 0.5
      });

      // Popup
      const durumAdı = this.getDurumAdi(gercekDurum);
      const konumNotu = kesinKonum ? '' : '<small style="color:#9ca3af"><i>📍 Kesin konum girilmemiş, il merkezi gösteriliyor</i></small><br/>';
      const popupContent = `
        <div class="popup-content">
          <strong>${olay.takipNo || 'Takip No'}</strong><br/>
          <small><strong>Konu:</strong> ${olay.konuAd || '-'}</small><br/>
          <small><strong>Kuruluş:</strong> ${olay.organizatorAd || '-'}</small><br/>
          <small><strong>Türü:</strong> ${olay.olayTuru || '-'}</small><br/>
          <small><strong>Yer:</strong> ${olay.il || ''} / ${olay.ilce || ''} - ${olay.mekan || ''}</small><br/>
          <small><strong>Tarih:</strong> ${new Date(olay.tarih).toLocaleDateString('tr-TR')}</small><br/>
          <small><strong>Durumu:</strong> ${durumAdı}</small><br/>
          <small><strong>Katılımcı:</strong> ${olay.katilimciSayisi || '0'}</small><br/>
          ${konumNotu}
        </div>
      `;

      marker.bindPopup(popupContent);
      this.olayMarkersGroup.addLayer(marker);
      gosterilenSayi++;
    });

    console.log(`${gosterilenSayi}/${olaylar.length} olay haritada gösteriliyor (kesin konum olmayan il merkezine yerleştirildi)`);

    // Harita zoom'u otomatik ayarla (tüm marker'lara sığsın)
    if (gosterilenSayi > 0) {
      this.fitMapToMarkers();
    }
  }

  /**
   * Olayın gerçek durumunu (Planlandi/Devam Ediyor/Gerceklesti) belirle
   * Durum enum'u ile tarih bilgileri birleştirilerek hesaplanır
   */
  private getGercekDurum(olay: OlayData): number {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    // Iptal ise direkt dön
    if (olay.durum === OlayDurumEnum.Iptal) {
      return OlayDurumEnum.Iptal;
    }

    // Gerçek başlangıç tarihi varsa ve bugüne eşit/geçti
    if (olay.gercekBaslangicTarihi) {
      const baslangicTarihi = new Date(olay.gercekBaslangicTarihi);
      baslangicTarihi.setHours(0, 0, 0, 0);

      if (baslangicTarihi <= today) {
        // Bitış tarihi yok veya bugün/gelecekte → hâlâ devam ediyor
        if (!olay.gercekBitisTarihi) {
          return OlayDurumEnum.DevamEdiyor;
        }

        const bitisTarihi = new Date(olay.gercekBitisTarihi);
        bitisTarihi.setHours(0, 0, 0, 0);

        // Bitış tarihi geçti = tamamlandı
        if (bitisTarihi < today) {
          return OlayDurumEnum.Gerceklesti;
        }

        // Bitış tarihi bugün veya gelecekte = devam ediyor
        return OlayDurumEnum.DevamEdiyor;
      }
    }

    // Başlangıç tarihi gelmemişse = planlandi
    return OlayDurumEnum.Planlandi;
  }

  /**
   * Duruma göre renk belirle
   */
  private getColorByDurum(durum: number): { fill: string; border: string } {
    switch (durum) {
      case OlayDurumEnum.Planlandi:
        return { fill: '#3b82f6', border: '#1e40af' }; // Mavi
      case OlayDurumEnum.DevamEdiyor:
        return { fill: '#f59e0b', border: '#b45309' }; // Turuncu
      case OlayDurumEnum.Gerceklesti:
        return { fill: '#ef4444', border: '#dc2626' }; // Kırmızı
      case OlayDurumEnum.Iptal:
        return { fill: '#9ca3af', border: '#6b7280' }; // Gri
      default:
        return { fill: '#6b7280', border: '#4b5563' }; // Varsayılan gri
    }
  }

  /**
   * Durum adını döndür
   */
  private getDurumAdi(durum: number): string {
    switch (durum) {
      case OlayDurumEnum.Planlandi:
        return 'Planlandı';
      case OlayDurumEnum.DevamEdiyor:
        return 'Devam Ediyor';
      case OlayDurumEnum.Gerceklesti:
        return 'Gerçekleşti';
      case OlayDurumEnum.Iptal:
        return 'İptal';
      default:
        return 'Bilinmiyor';
    }
  }

  /**
   * Harita zoom'u marker'lara sığacak şekilde ayarla
   */
  private fitMapToMarkers(): void {
    try {
      const bounds = this.olayMarkersGroup.getBounds();
      if (bounds.isValid()) {
        this.map.fitBounds(bounds, { padding: [50, 50], maxZoom: 13 });
      }
    } catch (error) {
      console.error('Marker bounds ayarlama hatası:', error);
    }
  }

  /**
   * Legend'i navbar'a enjekte et
   */
  private injectLegendToNavbar(): void {
    try {
      const navbarCenter = document.getElementById('navbar-center-outlet');
      if (!navbarCenter) return;

      navbarCenter.innerHTML = '';

      const legend = document.createElement('div');
      legend.className = 'navbar-legend';
      legend.style.display = 'flex';
      legend.style.alignItems = 'center';
      legend.style.gap = '16px';
      legend.style.paddingRight = '20px';

      // Planlandi (Mavi)
      const planlandiItem = document.createElement('div');
      planlandiItem.style.display = 'flex';
      planlandiItem.style.alignItems = 'center';
      planlandiItem.style.gap = '6px';
      planlandiItem.style.fontSize = '12px';
      planlandiItem.style.color = '#555';
      planlandiItem.style.whiteSpace = 'nowrap';

      const planlandiMarker = document.createElement('span');
      planlandiMarker.style.width = '8px';
      planlandiMarker.style.height = '8px';
      planlandiMarker.style.borderRadius = '50%';
      planlandiMarker.style.backgroundColor = '#3b82f6';
      planlandiMarker.style.border = '1px solid #1e40af';

      planlandiItem.appendChild(planlandiMarker);
      planlandiItem.appendChild(document.createTextNode('Planlandı'));
      legend.appendChild(planlandiItem);

      // Devam Ediyor (Turuncu)
      const devamItem = document.createElement('div');
      devamItem.style.display = 'flex';
      devamItem.style.alignItems = 'center';
      devamItem.style.gap = '6px';
      devamItem.style.fontSize = '12px';
      devamItem.style.color = '#555';
      devamItem.style.whiteSpace = 'nowrap';

      const devamMarker = document.createElement('span');
      devamMarker.style.width = '8px';
      devamMarker.style.height = '8px';
      devamMarker.style.borderRadius = '50%';
      devamMarker.style.backgroundColor = '#f59e0b';
      devamMarker.style.border = '1px solid #b45309';

      devamItem.appendChild(devamMarker);
      devamItem.appendChild(document.createTextNode('Devam Ediyor'));
      legend.appendChild(devamItem);

      // Gerceklesti (Kırmızı)
      const gerceklestiItem = document.createElement('div');
      gerceklestiItem.style.display = 'flex';
      gerceklestiItem.style.alignItems = 'center';
      gerceklestiItem.style.gap = '6px';
      gerceklestiItem.style.fontSize = '12px';
      gerceklestiItem.style.color = '#555';
      gerceklestiItem.style.whiteSpace = 'nowrap';

      const gerceklestiMarker = document.createElement('span');
      gerceklestiMarker.style.width = '8px';
      gerceklestiMarker.style.height = '8px';
      gerceklestiMarker.style.borderRadius = '50%';
      gerceklestiMarker.style.backgroundColor = '#ef4444';
      gerceklestiMarker.style.border = '1px solid #dc2626';

      gerceklestiItem.appendChild(gerceklestiMarker);
      gerceklestiItem.appendChild(document.createTextNode('Gerçekleşti'));
      legend.appendChild(gerceklestiItem);

      // Iptal (Gri)
      const iptalItem = document.createElement('div');
      iptalItem.style.display = 'flex';
      iptalItem.style.alignItems = 'center';
      iptalItem.style.gap = '6px';
      iptalItem.style.fontSize = '12px';
      iptalItem.style.color = '#555';
      iptalItem.style.whiteSpace = 'nowrap';

      const iptalMarker = document.createElement('span');
      iptalMarker.style.width = '8px';
      iptalMarker.style.height = '8px';
      iptalMarker.style.borderRadius = '50%';
      iptalMarker.style.backgroundColor = '#9ca3af';
      iptalMarker.style.border = '1px solid #6b7280';

      iptalItem.appendChild(iptalMarker);
      iptalItem.appendChild(document.createTextNode('İptal'));
      legend.appendChild(iptalItem);

      navbarCenter.appendChild(legend);
    } catch (error) {
      console.error('Legend enjeksiyonu hatası:', error);
    }
  }

  /**
   * Filtreyi sıfırla
   */
  onReset(): void {
    this.applyDefaultFilter();
  }

  toggleVIPZiyaretler(): void {
    this.showVIPZiyaretler = !this.showVIPZiyaretler;
    if (this.showVIPZiyaretler) {
      this.vipMarkersGroup.addTo(this.map);
      this.geoService.getVIPZiyaretlerForMap(
        this.selectedProvince || undefined
      ).subscribe((data: any) => this.displayVIPZiyaretler(data));
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

  /**
   * Harita verilerini yükle
   */
  private loadMapData(): void {
    this.applyDefaultFilter();
  }

  /**
   * VIP ziyaretçileri göster
   */
  private displayVIPZiyaretler(data: any): void {
    if (!data || data.length === 0) return;

    this.vipMarkersGroup.clearLayers();
    // VIP marker'larını ekle
    console.log('VIP ziyaretçiler gösteriliyor');
  }

  /**
   * İl sınırlarını yükle
   */
  private loadProvinceBoundaries(): void {
    // İl sınır verilerini yükle
    console.log('İl sınırları yükleniyor');
  }
}
