import { Injectable } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Item } from '../interfaces/item';
import { Unit } from '../interfaces/unit';
import { Category, SubCategory } from '../interfaces/category';

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
  private silentHeaders!: HttpHeaders;

  private itemsSubject = new BehaviorSubject<Item[]>([]);
  items$ = this.itemsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.headers = new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
      api_key: this.apiKey,
    });
    this.silentHeaders = this.headers.set('X-Skip-Error-Toast', 'true');
  }

  loadItems() {
    return this.http
      .get<any>(`${this.apiUrl}/Product/ProductList`, { headers: this.headers })
      .pipe(
        tap((res) => {
          if (res?.data) {
            this.itemsSubject.next(res.data);
          }
        })
      );
  }

  loadUnits() {
    return this.http.get<Unit[]>(`${this.apiUrl}/Unit`, { headers: this.headers });
  }

  loadCategories() {
    return this.http.get<Category[]>(`${this.apiUrl}/Product/Categories`, { headers: this.silentHeaders });
  }

  loadSubCategories() {
    return this.http.get<SubCategory[]>(`${this.apiUrl}/Product/SubCategories`, { headers: this.silentHeaders });
  }

  createItem(item: Item) {
    return this.http
      .post<any>(`${this.apiUrl}/Product/AddProduct`, item, { headers: this.headers })
      .pipe(
        tap((res) => {
          if (res?.success && res?.data) {
            this.itemsSubject.next([
              ...(this.itemsSubject.value ?? []),
              { ...item, ...res.data, categorie: item.categorie },
            ]);
          }
        })
      );
  }

  deleteItem(id: string) {
    return this.http
      .delete<any>(`${this.apiUrl}/Product/RemoveProduct`, {
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
      .put<any>(`${this.apiUrl}/Product/UpdateProduct`, item, { headers: this.headers })
      .pipe(
        tap((res) => {
          if (res?.success && res?.data) {
            const current = this.itemsSubject.value ?? [];
            const updatedItem = { ...item, ...res.data, categorie: item.categorie };
            const updated = current.map((i) => (i.id === res.data.id ? updatedItem : i));
            this.itemsSubject.next(updated);
          }
        })
      );
  }
}

