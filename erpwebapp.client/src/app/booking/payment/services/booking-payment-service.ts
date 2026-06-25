import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map, tap } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { BookingPayment } from '../interfaces/booking-payment';

@Injectable({ providedIn: 'root' })
export class BookingPaymentService {
  private readonly paymentsSubject = new BehaviorSubject<BookingPayment[]>([]);
  readonly payments$ = this.paymentsSubject.asObservable();

  private get apiUrl(): string {
    return `${environment.apiUrl}/BookingPayment`;
  }

  private get headers(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
      api_key: environment.apiKey,
    });
  }

  constructor(private http: HttpClient) {}

  loadPayments(bookingId?: number) {
    const url = bookingId ? `${this.apiUrl}?bookingId=${bookingId}` : this.apiUrl;
    return this.http.get<any>(url, { headers: this.headers }).pipe(
      map((response) => this.unwrapArray(response).map((x) => this.normalize(x))),
      tap((payments) => this.paymentsSubject.next(payments))
    );
  }

  createPayment(payment: BookingPayment) {
    return this.http
      .post<any>(this.apiUrl, this.toPayload(payment), { headers: this.headers })
      .pipe(
        map((response) => this.normalize(response?.data ?? response)),
        tap((created) =>
          this.paymentsSubject.next([created, ...this.paymentsSubject.value])
        )
      );
  }

  updatePayment(payment: BookingPayment) {
    return this.http
      .put<any>(`${this.apiUrl}/${payment.id}`, this.toPayload(payment), {
        headers: this.headers,
      })
      .pipe(
        map((response) => this.normalize(response?.data ?? response)),
        tap((updated) =>
          this.paymentsSubject.next(
            this.paymentsSubject.value.map((current) =>
              current.id === updated.id ? updated : current
            )
          )
        )
      );
  }

  deletePayment(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.headers,
    }).pipe(
      tap(() =>
        this.paymentsSubject.next(
          this.paymentsSubject.value.filter((payment) => payment.id !== id)
        )
      )
    );
  }

  private toPayload(payment: BookingPayment) {
    return {
      id: payment.id || 0,
      bookingId: Number(payment.bookingId),
      amount: Number(payment.amount),
      paymentDate: payment.paymentDate,
      paymentMethod: payment.paymentMethod,
      status: payment.status,
      transactionReference: payment.transactionReference?.trim() || '',
      notes: payment.notes?.trim() || null,
    };
  }

  private unwrapArray(value: any): any[] {
    const data = value?.data ?? value?.Data ?? value;
    return Array.isArray(data) ? data : [];
  }

  private normalize(value: any): BookingPayment {
    return {
      id: value?.id ?? value?.Id ?? 0,
      bookingId: value?.bookingId ?? value?.BookingId ?? 0,
      bookingNumber: value?.bookingNumber ?? value?.BookingNumber ?? '',
      customerName: value?.customerName ?? value?.CustomerName ?? '',
      bookingAmount: value?.bookingAmount ?? value?.BookingAmount ?? 0,
      amount: value?.amount ?? value?.Amount ?? 0,
      paymentDate: value?.paymentDate ?? value?.PaymentDate ?? '',
      paymentMethod: value?.paymentMethod ?? value?.PaymentMethod ?? '',
      status: value?.status ?? value?.Status ?? 'Paid',
      transactionReference:
        value?.transactionReference ?? value?.TransactionReference ?? '',
      notes: value?.notes ?? value?.Notes ?? null,
      createdDate: value?.createdDate ?? value?.CreatedDate ?? '',
    };
  }
}
