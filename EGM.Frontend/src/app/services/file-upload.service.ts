import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface FileUploadResponse {
  url: string;
  fileName: string;
  systemName: string;
}

@Injectable({ providedIn: 'root' })
export class FileUploadService {
  private apiUrl = environment.apiUrl + '/api/file/upload';

  constructor(private http: HttpClient) {}

  upload(file: File): Observable<FileUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<FileUploadResponse>(this.apiUrl, formData);
  }
}
