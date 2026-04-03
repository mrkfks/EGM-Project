import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

export interface Konu {
  id: string;
  ad: string;
  aciklama?: string;
  tur?: string;
  ustKonuId?: string;
  ustKonuAd?: string;
}

@Injectable({ providedIn: 'root' })
export class KonuService {
  private apiUrl = '/api/organizator/konu';
  private cache$?: Observable<Konu[]>;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Konu[]> {
    if (!this.cache$) {
      this.cache$ = this.http.get<Konu[]>(this.apiUrl).pipe(shareReplay(1));
    }
    return this.cache$;
  }

  invalidateCache(): void { this.cache$ = undefined; }
}
