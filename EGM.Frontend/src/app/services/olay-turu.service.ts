import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface OlayTuru {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class OlayTuruService {
  private apiUrl = environment.apiUrl + '/api/OlayTuru';
  private cache$?: Observable<OlayTuru[]>;

  constructor(private http: HttpClient) {}

  getAll(): Observable<OlayTuru[]> {
    if (!this.cache$) {
      this.cache$ = this.http.get<OlayTuru[]>(this.apiUrl).pipe(shareReplay(1));
    }
    return this.cache$;
  }

  invalidateCache(): void { this.cache$ = undefined; }

  get(id: string): Observable<OlayTuru> {
    return this.http.get<OlayTuru>(`${this.apiUrl}/${id}`);
  }

  create(model: Partial<OlayTuru>): Observable<OlayTuru> {
    this.invalidateCache();
    return this.http.post<OlayTuru>(this.apiUrl, model);
  }

  update(id: string, model: Partial<OlayTuru>): Observable<void> {
    this.invalidateCache();
    return this.http.put<void>(`${this.apiUrl}/${id}`, model);
  }

  delete(id: string): Observable<void> {
    this.invalidateCache();
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
