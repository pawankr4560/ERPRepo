import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { UnpaidInstallment } from './loan-payment-service';

export interface RazorpayConfig {
  keyId: string;
}

export interface UserLoanSummary {
  id: number;
  loanNumber: string;
  loanAmount: number;
  emi: number;
  tenure: number;
  status: string;
  unpaidInstallments: number;
  nextDueDate?: string;
  nextEmiAmount?: number;
}

export interface RazorpayEmiOrder {
  keyId: string;
  orderId: string;
  amount: number;
  amountPaise: number;
  currency: string;
  loanNumber: string;
  installmentNo: number;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
}

export interface RazorpayEmiVerifyRequest {
  loanId: number;
  scheduleId: number;
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

export interface RazorpayEmiVerifyResponse {
  success: boolean;
  message: string;
}

export interface RazorpayCheckoutResponse {
  razorpay_order_id: string;
  razorpay_payment_id: string;
  razorpay_signature: string;
}

declare global {
  interface Window {
    Razorpay?: new (options: Record<string, unknown>) => { open: () => void };
  }
}

@Injectable({
  providedIn: 'root',
})
export class RazorpayService {
  private readonly apiUrl = `${environment.apiUrl}/Razorpay`;

  constructor(private http: HttpClient) {}

  getConfig(): Observable<RazorpayConfig> {
    return this.http
      .get<any>(`${this.apiUrl}/config`, { headers: this.headers })
      .pipe(map((response) => this.normalizeConfig(response)));
  }

  getMyLoans(): Observable<UserLoanSummary[]> {
    return this.http
      .get<any[]>(`${this.apiUrl}/my-loans`, { headers: this.headers })
      .pipe(map((response) => (response ?? []).map((item) => this.normalizeLoan(item))));
  }

  getUnpaidInstallments(loanId: number): Observable<UnpaidInstallment[]> {
    return this.http
      .get<any[]>(`${this.apiUrl}/unpaid-installments/${loanId}`, { headers: this.headers })
      .pipe(
        map((response) =>
          (response ?? []).map((installment) => this.normalizeInstallment(installment))
        )
      );
  }

  createEmiOrder(loanId: number, scheduleId: number): Observable<RazorpayEmiOrder> {
    return this.http
      .post<any>(
        `${this.apiUrl}/emi/order`,
        { loanId, scheduleId },
        { headers: this.headers }
      )
      .pipe(map((response) => this.normalizeOrder(response)));
  }

  verifyEmiPayment(request: RazorpayEmiVerifyRequest): Observable<RazorpayEmiVerifyResponse> {
    return this.http
      .post<any>(`${this.apiUrl}/emi/verify`, request, { headers: this.headers })
      .pipe(map((response) => this.normalizeVerifyResponse(response)));
  }

  openCheckout(
    order: RazorpayEmiOrder,
    loanId: number,
    scheduleId: number,
    onSuccess: () => void,
    onError: (message: string) => void
  ): void {
    if (!window.Razorpay) {
      onError('Razorpay checkout could not be loaded.');
      return;
    }

    const options: Record<string, unknown> = {
      key: order.keyId,
      amount: order.amountPaise,
      currency: order.currency,
      name: 'GKFIN',
      description: `EMI payment for ${order.loanNumber} · Installment ${order.installmentNo}`,
      order_id: order.orderId,
      prefill: {
        name: order.customerName,
        email: order.customerEmail,
        contact: order.customerPhone,
      },
      theme: {
        color: '#1f3a6f',
      },
      handler: (response: RazorpayCheckoutResponse) => {
        this.verifyEmiPayment({
          loanId,
          scheduleId,
          razorpayOrderId: response.razorpay_order_id,
          razorpayPaymentId: response.razorpay_payment_id,
          razorpaySignature: response.razorpay_signature,
        }).subscribe({
          next: (result) => {
            if (result.success) {
              onSuccess();
            } else {
              onError(result.message || 'Payment verification failed.');
            }
          },
          error: (error) => {
            onError(
              error?.error?.message ??
                error?.error?.Message ??
                'Payment verification failed.'
            );
          },
        });
      },
      modal: {
        ondismiss: () => onError('Payment was cancelled.'),
      },
    };

    const checkout = new window.Razorpay(options);
    checkout.open();
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

  private normalizeConfig(value: any): RazorpayConfig {
    return {
      keyId: value?.KeyId ?? value?.keyId ?? '',
    };
  }

  private normalizeLoan(value: any): UserLoanSummary {
    return {
      id: value?.Id ?? value?.id ?? 0,
      loanNumber: value?.LoanNumber ?? value?.loanNumber ?? '',
      loanAmount: value?.LoanAmount ?? value?.loanAmount ?? 0,
      emi: value?.Emi ?? value?.emi ?? 0,
      tenure: value?.Tenure ?? value?.tenure ?? 0,
      status: value?.Status ?? value?.status ?? '',
      unpaidInstallments: value?.UnpaidInstallments ?? value?.unpaidInstallments ?? 0,
      nextDueDate: value?.NextDueDate ?? value?.nextDueDate,
      nextEmiAmount: value?.NextEmiAmount ?? value?.nextEmiAmount,
    };
  }

  private normalizeInstallment(value: any): UnpaidInstallment {
    return {
      id: value?.ScheduleId ?? value?.scheduleId ?? 0,
      loanId: value?.LoanId ?? value?.loanId ?? 0,
      installmentNo: value?.InstallmentNo ?? value?.installmentNo ?? 0,
      dueDate: value?.DueDate ?? value?.dueDate ?? '',
      emiAmount: value?.EMIAmount ?? value?.emiAmount ?? 0,
      principalAmount: value?.PrincipalAmount ?? value?.principalAmount ?? 0,
      interestAmount: value?.InterestAmount ?? value?.interestAmount ?? 0,
      outstandingBalance:
        value?.OutstandingBalance ?? value?.outstandingBalance ?? 0,
    };
  }

  private normalizeOrder(value: any): RazorpayEmiOrder {
    return {
      keyId: value?.KeyId ?? value?.keyId ?? '',
      orderId: value?.OrderId ?? value?.orderId ?? '',
      amount: value?.Amount ?? value?.amount ?? 0,
      amountPaise: value?.AmountPaise ?? value?.amountPaise ?? 0,
      currency: value?.Currency ?? value?.currency ?? 'INR',
      loanNumber: value?.LoanNumber ?? value?.loanNumber ?? '',
      installmentNo: value?.InstallmentNo ?? value?.installmentNo ?? 0,
      customerName: value?.CustomerName ?? value?.customerName ?? '',
      customerEmail: value?.CustomerEmail ?? value?.customerEmail ?? '',
      customerPhone: value?.CustomerPhone ?? value?.customerPhone ?? '',
    };
  }

  private normalizeVerifyResponse(value: any): RazorpayEmiVerifyResponse {
    return {
      success: value?.Success ?? value?.success ?? false,
      message: value?.Message ?? value?.message ?? '',
    };
  }
}
