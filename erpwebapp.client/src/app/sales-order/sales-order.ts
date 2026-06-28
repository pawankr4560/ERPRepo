import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SalesCheckoutRequest, SalesItem, SalesOrderService } from './sales-order.service';

interface CartLine {
  item: SalesItem;
  quantity: number;
}

@Component({
  selector: 'app-sales-order',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './sales-order.html',
  styleUrl: './sales-order.css',
})
export class SalesOrderComponent implements OnInit {
  items: SalesItem[] = [];
  cart: CartLine[] = [];
  orderType: 'Delivery' | 'Pickup' = 'Delivery';
  address = '';
  search = '';
  isLoading = false;
  isCheckingOut = false;
  lastServiceFee = 0;
  lastTotal = 0;

  constructor(
    private salesOrderService: SalesOrderService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadItems();
  }

  get filteredItems(): SalesItem[] {
    const query = this.search.trim().toLowerCase();
    if (!query) {
      return this.items;
    }

    return this.items.filter((item) =>
      [item.name, item.code, item.category].some((value) =>
        String(value ?? '').toLowerCase().includes(query)
      )
    );
  }

  get subtotal(): number {
    return this.cart.reduce((sum, line) => sum + line.item.price * line.quantity, 0);
  }

  get canCheckout(): boolean {
    return (
      this.cart.length > 0 &&
      !this.isCheckingOut &&
      (this.orderType === 'Pickup' || !!this.address.trim())
    );
  }

  loadItems(): void {
    this.isLoading = true;
    this.salesOrderService.getItems().subscribe({
      next: (items) => {
        this.items = items;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.notify(error?.error?.message ?? 'Unable to load sales items.');
      },
    });
  }

  addToCart(item: SalesItem): void {
    const existing = this.cart.find((line) => line.item.id === item.id);
    if (existing) {
      this.increment(existing);
      return;
    }

    this.cart.push({ item, quantity: 1 });
  }

  increment(line: CartLine): void {
    if (line.quantity >= line.item.stockQty) {
      this.notify(`Only ${line.item.stockQty} available for ${line.item.name}.`);
      return;
    }

    line.quantity += 1;
  }

  decrement(line: CartLine): void {
    line.quantity -= 1;
    if (line.quantity <= 0) {
      this.remove(line);
    }
  }

  remove(line: CartLine): void {
    this.cart = this.cart.filter((item) => item !== line);
  }

  checkout(): void {
    if (!this.canCheckout) {
      this.notify('Add items and complete order details before checkout.');
      return;
    }

    const request = this.buildRequest();
    this.isCheckingOut = true;
    this.salesOrderService.createCheckout(request).subscribe({
      next: (checkout) => {
        this.lastServiceFee = checkout.serviceFeeAmount;
        this.lastTotal = checkout.totalAmount;
        this.salesOrderService.openCheckout(
          checkout,
          request,
          () => {
            this.isCheckingOut = false;
            this.notify('Order placed successfully.');
            this.cart = [];
            this.address = '';
            this.lastServiceFee = 0;
            this.lastTotal = 0;
            this.loadItems();
          },
          (message) => {
            this.isCheckingOut = false;
            if (message !== 'Payment was cancelled.') {
              this.notify(message);
            }
          }
        );
      },
      error: (error) => {
        this.isCheckingOut = false;
        this.notify(error?.error?.message ?? error?.error?.errorMessage ?? 'Unable to start checkout.');
      },
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 2,
    }).format(value);
  }

  private buildRequest(): SalesCheckoutRequest {
    return {
      orderType: this.orderType,
      address: this.orderType === 'Pickup' ? '' : this.address.trim(),
      items: this.cart.map((line) => ({
        productId: line.item.id,
        quantity: line.quantity,
      })),
    };
  }

  private notify(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3500,
      horizontalPosition: 'right',
      verticalPosition: 'top',
    });
  }
}
