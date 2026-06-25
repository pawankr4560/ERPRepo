import { Injectable } from '@angular/core';
import { Item } from '../interfaces/item';
import { BehaviorSubject, tap } from 'rxjs';
import { ApiService } from '../../shared/services/api.service';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
 private productsSubject = new BehaviorSubject<Item[]>([]);
 products$ = this.productsSubject.asObservable();

constructor(private api: ApiService) {}
    /** Load products from API */
  loadProducts() {
    return this.api.get<any>('Product/ProductList')
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
    return this.api.delete<any>(
      `Product/RemoveProduct?id=${encodeURIComponent(id)}`
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
  return this.api.put<any>(
    'Product/UpdateProduct',
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
