import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize, forkJoin, Subject, takeUntil, timer } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { ToastService } from '../shared/services/toast.service';
import {
  ConstructionDelivery,
  ConstructionOrder,
  ConstructionQuote,
  StatusOption,
} from './construction-admin.models';
import { ConstructionAdminService } from './construction-admin.service';

@Component({
  selector: 'app-construction-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTabsModule,
    MatTooltipModule,
  ],
  templateUrl: './construction-admin.html',
  styleUrl: './construction-admin.css',
})
export class ConstructionAdminComponent implements OnInit, OnDestroy {
  quotes: ConstructionQuote[] = [];
  orders: ConstructionOrder[] = [];
  deliveries: ConstructionDelivery[] = [];
  search = '';
  isLoading = false;
  isSaving = false;
  selectedTab = 0;
  currentUser = { userId: '', customerName: '', contactNumber: '' };

  editingQuote?: ConstructionQuote;
  editingOrder?: ConstructionOrder;
  editingDelivery?: ConstructionDelivery;
  quotePrice: number | null = null;
  orderStatus: number | null = null;
  deliveryStatus: number | null = null;
  vehicleNumber = '';
  driverName = '';
  estimatedArrivalTime = '';

  readonly orderStatuses: StatusOption[] = [
    { value: 0, label: 'Placed' },
    { value: 1, label: 'Confirmed' },
    { value: 2, label: 'Processing' },
    { value: 3, label: 'Shipped' },
    { value: 4, label: 'Delivered' },
    { value: 5, label: 'Cancelled' },
  ];
  readonly deliveryStatuses: StatusOption[] = [
    { value: 0, label: 'Preparing' },
    { value: 1, label: 'Dispatched' },
    { value: 2, label: 'Out for Delivery' },
    { value: 3, label: 'Delivered' },
    { value: 4, label: 'Delayed' },
    { value: 5, label: 'Cancelled' },
  ];

  private readonly destroy$ = new Subject<void>();
  private isRefreshing = false;

  constructor(
    private constructionService: ConstructionAdminService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadCurrentUserFromToken();
    this.refresh();
    timer(10_000, 10_000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (!document.hidden && !this.isSaving) {
          this.refresh(true);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get filteredQuotes(): ConstructionQuote[] {
    return this.filter(this.quotes, (item) => [
      item.quoteId, item.productName,
      item.categoryName, item.status, item.deliveryLocation,
    ]);
  }

  get filteredOrders(): ConstructionOrder[] {
    return this.filter(this.orders, (item) => [
      item.orderId, item.quoteId,
      item.productName, item.status, item.deliveryLocation,
    ]);
  }

  get filteredDeliveries(): ConstructionDelivery[] {
    return this.filter(this.deliveries, (item) => [
      item.deliveryId, item.orderId,
      item.productName, item.vehicleNumber, item.driverName, item.status,
    ]);
  }

  get pendingQuoteCount(): number {
    return this.quotes.filter((item) => ['Pending', 'PriceShared'].includes(item.status)).length;
  }

  get activeOrderCount(): number {
    return this.orders.filter((item) => !['Delivered', 'Cancelled'].includes(item.status)).length;
  }

  get activeDeliveryCount(): number {
    return this.deliveries.filter((item) => !['Delivered', 'Cancelled'].includes(item.status)).length;
  }

  get availableOrderStatuses(): StatusOption[] {
    if (!this.editingOrder) return [];
    const transitions: Record<string, number[]> = {
      Placed: [1, 5], Confirmed: [2, 5], Processing: [3, 5], Shipped: [4, 5],
    };
    const allowed = transitions[this.editingOrder.status] ?? [];
    return this.orderStatuses.filter((item) => allowed.includes(item.value));
  }

  get availableDeliveryStatuses(): StatusOption[] {
    if (!this.editingDelivery) return [];
    const transitions: Record<string, number[]> = {
      Preparing: [1, 4, 5],
      Dispatched: [2, 4, 5],
      'Out for Delivery': [3, 4, 5],
      OutForDelivery: [3, 4, 5],
      Delayed: [0, 1, 2, 5],
    };
    const allowed = transitions[this.editingDelivery.status] ?? [];
    return this.deliveryStatuses.filter((item) => allowed.includes(item.value));
  }

  get deliveryDispatchDetailsRequired(): boolean {
    return this.deliveryStatus === 1 || this.deliveryStatus === 2;
  }

  refresh(silent = false): void {
    if (this.isRefreshing) return;
    this.isRefreshing = true;
    if (!silent) this.isLoading = true;
    forkJoin({
      quotes: this.constructionService.getQuotes(),
      orders: this.constructionService.getOrders(),
      deliveries: this.constructionService.getDeliveries(),
    })
      .pipe(finalize(() => {
        this.isRefreshing = false;
        if (!silent) this.isLoading = false;
      }), takeUntil(this.destroy$))
      .subscribe({
        next: ({ quotes, orders, deliveries }) => {
          this.quotes = quotes.data ?? [];
          this.orders = orders.data ?? [];
          this.deliveries = deliveries.data ?? [];
        },
        error: (error) => this.showError(error, 'Construction records could not be loaded.'),
      });
  }

  editQuote(quote: ConstructionQuote): void {
    this.closeEditor();
    this.editingQuote = quote;
    this.quotePrice = quote.finalQuotedAmount > 0 ? quote.finalQuotedAmount : null;
  }

  editOrder(order: ConstructionOrder): void {
    this.closeEditor();
    this.editingOrder = order;
  }

  editDelivery(delivery: ConstructionDelivery): void {
    this.closeEditor();
    this.editingDelivery = delivery;
    this.vehicleNumber = delivery.vehicleNumber ?? '';
    this.driverName = delivery.driverName ?? '';
    this.deliveryStatus = this.recommendedDeliveryStatus(delivery.status);
  }

  closeEditor(): void {
    this.editingQuote = undefined;
    this.editingOrder = undefined;
    this.editingDelivery = undefined;
    this.quotePrice = null;
    this.orderStatus = null;
    this.deliveryStatus = null;
    this.vehicleNumber = '';
    this.driverName = '';
    this.estimatedArrivalTime = '';
  }

  saveQuote(form: NgForm): void {
    form.control.markAllAsTouched();
    if (form.invalid || !this.editingQuote || !this.quotePrice || this.isSaving) return;
    this.save(
      this.constructionService.updateQuotePrice(this.editingQuote.quoteId, this.quotePrice),
      'Quote price updated successfully.'
    );
  }

  saveOrder(form: NgForm): void {
    form.control.markAllAsTouched();
    if (form.invalid || !this.editingOrder || this.orderStatus === null || this.isSaving) return;
    this.save(
      this.constructionService.updateOrderStatus(this.editingOrder.orderId, this.orderStatus),
      'Order status updated successfully.'
    );
  }

  saveDelivery(form: NgForm): void {
    form.control.markAllAsTouched();
    if (form.invalid || !this.editingDelivery || this.deliveryStatus === null || this.isSaving) return;

    const eta = this.estimatedArrivalTime
      ? new Date(this.estimatedArrivalTime).toISOString()
      : null;
    this.save(
      this.constructionService.updateDelivery(this.editingDelivery.deliveryId, {
        vehicleNumber: this.vehicleNumber.trim() || null,
        driverName: this.driverName.trim() || null,
        estimatedArrivalTime: eta,
        status: this.deliveryStatus,
      }),
      'Delivery details updated successfully.'
    );
  }

  statusClass(status: string): string {
    return `status-${(status || 'unknown').replace(/\s+/g, '-').toLowerCase()}`;
  }

  progressPercent(progress: number): number {
    return Math.round(Math.max(0, Math.min(1, Number(progress) || 0)) * 100);
  }

  nextDeliveryStep(status: string): string | null {
    const nextStatus = this.recommendedDeliveryStatus(status);
    return this.deliveryStatuses.find((item) => item.value === nextStatus)?.label ?? null;
  }

  canEditQuote(quote: ConstructionQuote): boolean {
    return !['Completed', 'Cancelled'].includes(quote.status);
  }

  canEditStatus(status: string): boolean {
    return !['Delivered', 'Cancelled'].includes(status);
  }

  hasDeliveryForOrder(orderId: number): boolean {
    return this.deliveries.some((delivery) => delivery.orderId === orderId);
  }

  createDelivery(order: ConstructionOrder): void {
    if (this.isSaving || this.hasDeliveryForOrder(order.orderId)) return;

    this.isSaving = true;
    this.constructionService.createDeliveryForOrder(order.orderId)
      .pipe(finalize(() => (this.isSaving = false)), takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.toastService.success(`Delivery setup created for order #${order.orderId}.`);
          this.closeEditor();
          this.selectedTab = 2;
          this.refresh();
        },
        error: (error) => this.showError(error, 'Delivery setup could not be created.'),
      });
  }

  private filter<T>(items: T[], fields: (item: T) => unknown[]): T[] {
    const query = this.search.trim().toLowerCase();
    if (!query) return items;
    return items.filter((item) =>
      fields(item).some((value) => String(value ?? '').toLowerCase().includes(query))
    );
  }

  private recommendedDeliveryStatus(status: string): number | null {
    const recommended: Record<string, number> = {
      Preparing: 1,
      Dispatched: 2,
      'Out for Delivery': 3,
      OutForDelivery: 3,
    };
    return recommended[status] ?? null;
  }

  private loadCurrentUserFromToken(): void {
    const token = localStorage.getItem('auth_token') ?? localStorage.getItem('jwt');
    if (!token) return;

    try {
      const claims = jwtDecode<Record<string, unknown>>(token);
      const firstName = String(claims['FirstName'] ?? '');
      const lastName = String(claims['LastName'] ?? '');
      this.currentUser = {
        userId: String(claims['Id'] ?? ''),
        customerName: `${firstName} ${lastName}`.trim(),
        contactNumber: String(claims['Phone'] ?? ''),
      };
    } catch {
      this.currentUser = { userId: '', customerName: '', contactNumber: '' };
    }
  }

  private save(request: ReturnType<ConstructionAdminService['updateQuotePrice']>, message: string): void {
    this.isSaving = true;
    request.pipe(finalize(() => (this.isSaving = false)), takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toastService.success(message);
        this.closeEditor();
        this.refresh();
      },
      error: (error) => this.showError(error, 'The update could not be saved.'),
    });
  }

  private showError(error: any, fallback: string): void {
    const errors = error?.error?.errors;
    const validationMessage = errors ? (Object.values(errors).flat() as string[])[0] : null;
    this.toastService.error(
      error?.error?.errorMessage || error?.error?.message || validationMessage || fallback
    );
  }
}
