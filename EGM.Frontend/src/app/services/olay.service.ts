import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { OlayFilterRequest, OlayFilterResponse } from '../models/olay-filter.model';

@Injectable({ providedIn: 'root' })
export class OlayService {
  private apiUrl = environment.apiUrl + '/api/olay';

  constructor(private http: HttpClient) {}

  /**
   * Harita sayfası için gelişmiş filtreleme ile olayları getirir.
   * @param filter Filtre parametreleri
   * @returns Sayfalanmış olay listesi
   */
  getFilteredForMap(filter: OlayFilterRequest): Observable<OlayFilterResponse> {
    // Tarih'leri ISO string'e çevirme (eğer Date ise)
    const requestFilter: any = { ...filter };
    if (filter.tarihBaslangic instanceof Date) {
      requestFilter.tarihBaslangic = filter.tarihBaslangic.toISOString();
    }
    if (filter.tarihBitis instanceof Date) {
      requestFilter.tarihBitis = filter.tarihBitis.toISOString();
    }

    return this.http.post<OlayFilterResponse>(`${this.apiUrl}/filtre`, requestFilter);
  }
}
