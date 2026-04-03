import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';

export interface GerceklesmeSekli {
  id: string;
  name: string;
  olayTuruId: string;
}

@Injectable({ providedIn: 'root' })
export class GerceklesmeSekliService {
  private apiUrl = '/api/GerceklesmeSekli';
  private cache$?: Observable<GerceklesmeSekli[]>;

  constructor(private http: HttpClient) {}

  getAll(): Observable<GerceklesmeSekli[]> {
    if (!this.cache$) {
      this.cache$ = this.http.get<GerceklesmeSekli[]>(this.apiUrl).pipe(shareReplay(1));
    }
    return this.cache$;
  }

  invalidateCache(): void { this.cache$ = undefined; }

  get(id: string): Observable<GerceklesmeSekli> {
    return this.http.get<GerceklesmeSekli>(`${this.apiUrl}/${id}`);
  }

  create(model: Partial<GerceklesmeSekli>): Observable<GerceklesmeSekli> {
    this.invalidateCache();
    return this.http.post<GerceklesmeSekli>(this.apiUrl, model);
  }

  update(id: string, model: Partial<GerceklesmeSekli>): Observable<void> {
    this.invalidateCache();
    return this.http.put<void>(`${this.apiUrl}/${id}`, model);
  }

  delete(id: string): Observable<void> {
    this.invalidateCache();
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
