import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { RazorpayCheckoutResponse } from '../transaction/services/razorpay-service';

export interface SalesItem {
  id: string;
  code: string;
  name: string;
  category: string;
  stockQty: number;
  price: number;
  image?: string | null;
  description?: string | null;
}

export interface SalesOrderLine {
  productId: string;
  quantity: number;
}

export interface SalesCheckoutRequest {
  orderType: 'Delivery' | 'Pickup';
  address: string;
  items: SalesOrderLine[];
}

export interface SalesOrderCheckout {
  keyId: string;
  orderId: string;
  subtotal: number;
  serviceFeeAmount: number;
  totalAmount: number;
  amountPaise: number;
  currency: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
}

export interface SalesOrderVerifyRequest extends SalesCheckoutRequest {
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

@Injectable({ providedIn: 'root' })
export class SalesOrderService {
  private readonly apiUrl = `${environment.apiUrl}/Order`;

  constructor(private http: HttpClient) {}

  getItems(): Observable<SalesItem[]> {
    return this.http
      .get<any>(`${this.apiUrl}/SalesItems`, { headers: this.headers })
      .pipe(map((response) => this.unwrapArray(response).map((item) => this.normalizeItem(item))));
  }

  createCheckout(request: SalesCheckoutRequest): Observable<SalesOrderCheckout> {
    return this.http
      .post<any>(`${this.apiUrl}/SalesCheckout`, request, { headers: this.headers })
      .pipe(map((response) => this.normalizeCheckout(response?.data ?? response?.Data ?? response)));
  }

  verifyPayment(request: SalesOrderVerifyRequest): Observable<{ success: boolean; message: string }> {
    return this.http
      .post<any>(`${this.apiUrl}/VerifySalesPayment`, request, { headers: this.headers })
      .pipe(
        map((response) => ({
          success: response?.success ?? response?.Success ?? response?.data?.success ?? true,
          message: response?.message ?? response?.Message ?? response?.data?.message ?? 'Order placed successfully.',
        }))
      );
  }

  openCheckout(
    checkout: SalesOrderCheckout,
    request: SalesCheckoutRequest,
    onSuccess: () => void,
    onError: (message: string) => void
  ): void {
    if (!window.Razorpay) {
      onError('Razorpay checkout could not be loaded.');
      return;
    }

    const options: Record<string, unknown> = {
      key: checkout.keyId,
      amount: checkout.amountPaise,
      currency: checkout.currency,
      name: 'FinVault',
      description: `${request.orderType} sales order`,
      order_id: checkout.orderId,
      prefill: {
        name: checkout.customerName,
        email: checkout.customerEmail,
        contact: checkout.customerPhone,
      },
      theme: {
        color: '#1f3a6f',
      },
      handler: (response: RazorpayCheckoutResponse) => {
        this.verifyPayment({
          ...request,
          razorpayOrderId: response.razorpay_order_id,
          razorpayPaymentId: response.razorpay_payment_id,
          razorpaySignature: response.razorpay_signature,
        }).subscribe({
          next: (result) => result.success ? onSuccess() : onError(result.message),
          error: (error) =>
            onError(error?.error?.message ?? error?.error?.Message ?? 'Payment verification failed.'),
        });
      },
      modal: {
        ondismiss: () => onError('Payment was cancelled.'),
      },
    };

    new window.Razorpay(options).open();
  }

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

  private unwrapArray(value: any): any[] {
    const data = value?.data ?? value?.Data ?? value;
    return Array.isArray(data) ? data : [];
  }

  private normalizeItem(value: any): SalesItem {
    return {
      id: value?.id ?? value?.Id ?? '',
      code: value?.code ?? value?.Code ?? '',
      name: value?.name ?? value?.Name ?? '',
      category: value?.category ?? value?.Category ?? '',
      stockQty: value?.stockQty ?? value?.StockQty ?? 0,
      price: value?.price ?? value?.Price ?? 0,
      image: value?.image ?? value?.Image ?? null,
      description: value?.description ?? value?.Description ?? null,
    };
  }

  private normalizeCheckout(value: any): SalesOrderCheckout {
    return {
      keyId: value?.keyId ?? value?.KeyId ?? '',
      orderId: value?.orderId ?? value?.OrderId ?? '',
      subtotal: value?.subtotal ?? value?.Subtotal ?? 0,
      serviceFeeAmount: value?.serviceFeeAmount ?? value?.ServiceFeeAmount ?? 0,
      totalAmount: value?.totalAmount ?? value?.TotalAmount ?? 0,
      amountPaise: value?.amountPaise ?? value?.AmountPaise ?? 0,
      currency: value?.currency ?? value?.Currency ?? 'INR',
      customerName: value?.customerName ?? value?.CustomerName ?? '',
      customerEmail: value?.customerEmail ?? value?.CustomerEmail ?? '',
      customerPhone: value?.customerPhone ?? value?.CustomerPhone ?? '',
    };
  }
}
