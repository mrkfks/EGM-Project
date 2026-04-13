import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface District {
  id: number;
  name: string;
}

export interface Coordinates {
  latitude: number;
  longitude: number;
}

export interface GeoJsonFeature {
  id?: string | number;
  type: string;
  geometry: {
    type: string;
    coordinates: any[];
  };
  properties: Record<string, any>;
}

export interface GeoJsonFeatureCollection {
  type: string;
  features: GeoJsonFeature[];
}

export interface Province {
  id: number;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class GeoService {
  private readonly apiUrl = `${environment.apiBaseUrl}/api/geo`;

  constructor(private http: HttpClient) {}

  /**
   * Harita için tüm illeri döner
   */
  getProvinces(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/provinces-geopackage`);
  }

  /**
   * Verilen ile ait ilçeleri döner
   */
  getDistrictsByProvince(provinceName: string): Observable<District[]> {
    return this.http.get<District[]>(`${this.apiUrl}/districts-geopackage`, {
      params: { province: provinceName }
    });
  }

  /**
   * Verilen mahalleri döner
   */
  getNeighborhoodsByDistrict(districtName: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/neighborhoods-geopackage`, {
      params: { district: districtName }
    });
  }

  /**
   * İl, ilçe, mahalle adından koordinatları döner (geometri merkezi)
   */
  getCoordinates(
    provinceName?: string,
    districtName?: string,
    neighborhoodName?: string,
    adminLevel?: number
  ): Observable<Coordinates> {
    const params: any = {};
    if (provinceName) params['provinceName'] = provinceName;
    if (districtName) params['districtName'] = districtName;
    if (neighborhoodName) params['neighborhoodName'] = neighborhoodName;
    if (adminLevel) params['adminLevel'] = adminLevel;

    return this.http.get<Coordinates>(`${this.apiUrl}/get-coordinates`, { params });
  }

  /**
   * Verilen koordinatların hangi il/ilçe'de olduğunu bulur (reverse lookup)
   */
  resolveLocation(latitude: number, longitude: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/resolve-location`, {
      params: { latitude, longitude }
    });
  }

  /**
   * Harita için tüm olayları GeoJSON FeatureCollection olarak döner
   */
  getOlaylarForMap(il?: string, olayTuru?: string, yil?: number): Observable<GeoJsonFeatureCollection> {
    const params: any = {};
    if (il) params['il'] = il;
    if (olayTuru) params['olay_turu'] = olayTuru;
    if (yil) params['yil'] = yil;

    return this.http.get<GeoJsonFeatureCollection>(`${this.apiUrl}/olaylar`, { params });
  }

  /**
   * Harita için tüm VIP ziyaretlerini GeoJSON FeatureCollection olarak döner
   */
  getVIPZiyaretlerForMap(il?: string): Observable<GeoJsonFeatureCollection> {
    const params: any = {};
    if (il) params['il'] = il;

    return this.http.get<GeoJsonFeatureCollection>(`${this.apiUrl}/vipziyaretler`, { params });
  }

  /**
   * Verilen ID'ye ait il/ilçe sınırlarını GeoJSON geometrisi olarak döner
   */
  getGeometry(id: number): Observable<GeoJsonFeature> {
    return this.http.get<GeoJsonFeature>(`${this.apiUrl}/provinces/${id}/geometry`);
  }

  /**
   * Verilen şehir ID'sine ait ilçeleri döner
   */
  getDistrictsByCityId(cityId: number): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/${cityId}`);
  }
}
