import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl.replace(/\/+$/, '');

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string): Observable<T> {
    return this.http.get<T>(this.toUrl(endpoint), { headers: this.headers });
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<T>(this.toUrl(endpoint), body, { headers: this.headers });
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(this.toUrl(endpoint), body, { headers: this.headers });
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(this.toUrl(endpoint), { headers: this.headers });
  }

  private get headers(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
      api_key: environment.apiKey,
    });
  }

  private toUrl(endpoint: string): string {
    if (/^https?:\/\//i.test(endpoint)) {
      return endpoint;
    }

    return `${this.baseUrl}/${endpoint.replace(/^\/+/, '')}`;
  }
}
