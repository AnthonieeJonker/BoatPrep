import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BoatData } from '../models/boat-data.model';

@Injectable({
  providedIn: 'root'
})
export class BoatService {
  private baseUrl = 'https://localhost:44342/api/BoatData';

  constructor(private http: HttpClient) { }

  uploadAndParseFile(file: File): Observable<BoatData> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<BoatData>(`${this.baseUrl}/parse`, formData);
  }
}
