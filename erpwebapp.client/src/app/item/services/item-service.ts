import { Injectable } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Item } from '../interfaces/item';

@Injectable({
  providedIn: 'root',
})
export class ItemService {
  get apiUrl(): string {
    return environment.apiUrl;
  }

  get apiKey(): string {
    return environment.apiKey;
  }

  private headers!: HttpHeaders;

  private itemsSubject = new BehaviorSubject<Item[]>([]);
  items$ = this.itemsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.headers = new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
      api_key: this.apiKey,
    });
  }

  loadItems() {
    return this.http
      .get<any>(`${this.apiUrl}/Item/ItemList`, { headers: this.headers })
      .pipe(
        tap((res) => {
          if (res?.data) {
            this.itemsSubject.next(res.data);
          }
        })
      );
  }

  createItem(item: Item) {
    return this.http
      .post<any>(`${this.apiUrl}/Item/CreateItem`, item, { headers: this.headers })
      .pipe(
        tap((res) => {
          if (res?.success && res?.data) {
            this.itemsSubject.next([...(this.itemsSubject.value ?? []), res.data]);
          }
        })
      );
  }

  deleteItem(id: string) {
    return this.http
      .delete<any>(`${this.apiUrl}/Item/RemoveItem`, {
        headers: this.headers,
        params: { id },
      })
      .pipe(
        tap((res) => {
          if (res?.success) {
            this.itemsSubject.next(
              (this.itemsSubject.value ?? []).filter((i) => i.id !== id)
            );
          }
        })
      );
  }

  updateItem(item: Item) {
    return this.http
      .put<any>(`${this.apiUrl}/Item/UpdateItem`, item, { headers: this.headers })
      .pipe(
        tap((res) => {
          if (res?.success && res?.data) {
            const current = this.itemsSubject.value ?? [];
            const updated = current.map((i) => (i.id === res.data.id ? res.data : i));
            this.itemsSubject.next(updated);
          }
        })
      );
  }
}

