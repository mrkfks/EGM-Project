import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Group {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class GroupService {
  private apiUrl = environment.apiUrl + '/api/group';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Group[]> {
    return this.http.get<Group[]>(this.apiUrl);
  }
}
