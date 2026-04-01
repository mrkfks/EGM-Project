import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface OlayTuru {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class OlayTuruService {
  private apiUrl = '/api/OlayTuru';

  constructor(private http: HttpClient) {}

  getAll(): Observable<OlayTuru[]> {
    return this.http.get<OlayTuru[]>(this.apiUrl);
  }

  get(id: string): Observable<OlayTuru> {
    return this.http.get<OlayTuru>(`${this.apiUrl}/${id}`);
  }

  create(model: Partial<OlayTuru>): Observable<OlayTuru> {
    return this.http.post<OlayTuru>(this.apiUrl, model);
  }

  update(id: string, model: Partial<OlayTuru>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, model);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
