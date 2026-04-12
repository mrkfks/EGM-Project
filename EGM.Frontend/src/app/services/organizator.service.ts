import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface Organizator {
  id: string;
  ad: string;
  tur?: string;
}

@Injectable({ providedIn: 'root' })
export class OrganizatorService {
  private apiUrl = environment.apiUrl + '/api/organizator';
  private cache$?: Observable<Organizator[]>;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Organizator[]> {
    if (!this.cache$) {
      this.cache$ = this.http.get<Organizator[]>(this.apiUrl).pipe(shareReplay(1));
    }
    return this.cache$;
  }

  invalidateCache(): void {
    this.cache$ = undefined;
  }

  get(id: string): Observable<Organizator> {
    return this.http.get<Organizator>(`${this.apiUrl}/${id}`);
  }
}
