import { Injectable } from '@angular/core';
import { MenuItem } from './menu-item';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class Menu {
    private STORAGE_KEY = 'app_menus';
      get apiUrl(): string {
      return environment.apiUrl;
    }
    get apiKey(): string {
      return environment.apiKey;
    }
private headers!: HttpHeaders;

constructor(private http: HttpClient) {
  this.headers = new HttpHeaders({
    'Content-Type': 'application/json; charset=utf-8',
    'api_key': this.apiKey, 
  });
}
  getMenus(): MenuItem[] {
    const data = localStorage.getItem(this.STORAGE_KEY);
    return data ? JSON.parse(data) : [];
  }

 initmenus(): Observable<any> {
  return this.http.get<any>(`${this.apiUrl}/Menu`)
    .pipe(
      map(res => res?.data ?? res)
    );
}

getAllMenus(): Observable<MenuItem[]> {
  return this.http.get<any>(`${this.apiUrl}/Menu/all`, { headers: this.headers })
    .pipe(map(res => res?.data ?? res ?? []));
}

createMenu(menu: Omit<MenuItem, 'id' | 'children'>): Observable<MenuItem> {
  return this.http.post<any>(`${this.apiUrl}/Menu`, menu, { headers: this.headers })
    .pipe(map(res => res?.data ?? res));
}

updateMenu(id: number, menu: Omit<MenuItem, 'id' | 'children'>): Observable<MenuItem> {
  return this.http.put<any>(`${this.apiUrl}/Menu/${id}`, menu, { headers: this.headers })
    .pipe(map(res => res?.data ?? res));
}

deleteMenu(id: number): Observable<any> {
  return this.http.delete<any>(`${this.apiUrl}/Menu/${id}`, { headers: this.headers })
    .pipe(map(res => res?.data ?? res));
}



  // initMenus() {
  //   if (!localStorage.getItem(this.STORAGE_KEY)) {
  //     localStorage.setItem(this.STORAGE_KEY, JSON.stringify(MENU_DATA));
  //   }
  // }


}
