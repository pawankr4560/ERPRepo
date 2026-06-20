import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Item } from '../interfaces/item';
import { BehaviorSubject, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  get apiUrl(): string {
  return environment.apiUrl;
}
get apiKey(): string {
  return environment.apiKey;
}

private headers!: HttpHeaders;
 private productsSubject = new BehaviorSubject<Item[]>([]);
 products$ = this.productsSubject.asObservable();

constructor(private http: HttpClient) {
  this.headers = new HttpHeaders({
    'Content-Type': 'application/json; charset=utf-8',
    'api_key': this.apiKey, 
  });
}
    /** Load products from API */
  loadProducts() {
    return this.http.get<any>(`${this.apiUrl}/api/Product/ProductList`)
      .pipe(
        tap(res => {
          if (res?.data) {
            this.productsSubject.next(res.data);
          }
        })
      );
  }

  /** Delete product */
  deleteProduct(id: string) {
    return this.http.delete<any>(
      `${this.apiUrl}/api/Product/RemoveProduct`,
      { params: { id } }
    ).pipe(
      tap(res => {
        if (res?.success) {
          const updated = this.productsSubject.value
            .filter((p: { id: string; }) => p.id !== id);
          this.productsSubject.next(updated);
        }
      })
    );
  }

  /** Update product locally (after dialog save) */
  
   updateProduct(updatedProduct: Item) {
  return this.http.put<any>(
    `${this.apiUrl}/api/Product/UpdateProduct`,
    updatedProduct
  ).pipe(
    tap(res => {
      if (res?.success && res.data) {

        const current = this.productsSubject.value;

        const updatedList = current.map(p =>
          p.id === res.data.id ? res.data : p
        );
        // ✅ correct array emission
        this.productsSubject.next(updatedList);
      }
    })
  );
}

}
