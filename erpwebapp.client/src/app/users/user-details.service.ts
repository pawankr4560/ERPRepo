import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { UserDetails } from './user-details';

@Injectable({ providedIn: 'root' })
export class UserDetailsService {
  constructor(private http: HttpClient) {}

  private get headers(): HttpHeaders {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
      api_key: environment.apiKey,
    } as any);

    const token = localStorage.getItem('jwt');
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return headers;
  }

  getAll() {
    return this.http.get<UserDetails[]>(`${environment.apiUrl}/api/UserDetails`, {
      headers: this.headers,
    });
  }

  create(user: Omit<UserDetails, 'id'>) {
    return this.http.post<UserDetails>(`${environment.apiUrl}/api/UserDetails`, user, {
      headers: this.headers,
    });
  }
}
