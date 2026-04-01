import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface GerceklesmeSekli {
  id: string;
  name: string;
  olayTuruId: string;
}

@Injectable({ providedIn: 'root' })
export class GerceklesmeSekliService {
  private apiUrl = '/api/GerceklesmeSekli';

  constructor(private http: HttpClient) {}

  getAll(): Observable<GerceklesmeSekli[]> {
    return this.http.get<GerceklesmeSekli[]>(this.apiUrl);
  }

  get(id: string): Observable<GerceklesmeSekli> {
    return this.http.get<GerceklesmeSekli>(`${this.apiUrl}/${id}`);
  }

  create(model: Partial<GerceklesmeSekli>): Observable<GerceklesmeSekli> {
    return this.http.post<GerceklesmeSekli>(this.apiUrl, model);
  }

  update(id: string, model: Partial<GerceklesmeSekli>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, model);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
