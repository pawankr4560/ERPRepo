import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoanPayment } from './loan-service';

export interface UnpaidInstallment {
  id: number;
  loanId: number;
  installmentNo: number;
  dueDate: string;
  emiAmount: number;
  principalAmount: number;
  interestAmount: number;
  outstandingBalance: number;
}

@Injectable({
  providedIn: 'root',
})
export class LoanPaymentService {
  private readonly apiUrl = `${environment.apiUrl}/LoanPayment`;
  private readonly headers = new HttpHeaders({
    'Content-Type': 'application/json; charset=utf-8',
    api_key: environment.apiKey,
  } as any);

  private readonly paymentsSubject = new BehaviorSubject<LoanPayment[]>([]);
  readonly payments$ = this.paymentsSubject.asObservable();

  constructor(private http: HttpClient) {}

  loadPayments() {
    return this.http.get<any[]>(this.apiUrl, { headers: this.headers }).pipe(
      map((response: any) =>
        (response?.data ?? response ?? []).map((payment: any) =>
          this.normalizePayment(payment)
        )
      ),
      tap((payments) => this.paymentsSubject.next(payments))
    );
  }

  getPayment(id: number) {
    return this.http
      .get<any>(`${this.apiUrl}/${id}`, { headers: this.headers })
      .pipe(map((response) => this.normalizePayment(response?.data ?? response)));
  }

  getUnpaidInstallments(loanNumber: string) {
    return this.http
      .get<any[]>(
        `${environment.apiUrl}/LoanEMISchedule/unpaid-installments/${encodeURIComponent(loanNumber)}`,
        { headers: this.headers }
      )
      .pipe(
        map((response: any) =>
          (response?.data ?? response ?? []).map((installment: any) =>
            this.normalizeUnpaidInstallment(installment)
          )
        )
      );
  }

  createPayment(payment: LoanPayment) {
    return this.http
      .post<any>(this.apiUrl, this.toPayload(payment), { headers: this.headers })
      .pipe(
        map((response) => this.normalizePayment(response?.data ?? response)),
        tap((created) =>
          this.paymentsSubject.next([...this.paymentsSubject.value, created])
        )
      );
  }

  updatePayment(payment: LoanPayment) {
    return this.http
      .put<any>(`${this.apiUrl}/${payment.id}`, this.toPayload(payment), {
        headers: this.headers,
      })
      .pipe(
        map((response) =>
          this.normalizePayment(response?.data ?? response ?? payment)
        ),
        tap((updated) => {
          const payments = this.paymentsSubject.value.map((item) =>
            item.id === payment.id ? { ...updated, id: payment.id } : item
          );
          this.paymentsSubject.next(payments);
        })
      );
  }

  deletePayment(id: number) {
    return this.http.delete<any>(`${this.apiUrl}/${id}`, { headers: this.headers }).pipe(
      tap(() => {
        this.paymentsSubject.next(
          this.paymentsSubject.value.filter((payment) => payment.id !== id)
        );
      })
    );
  }

  private toPayload(payment: LoanPayment) {
    return {
      id: payment.id ?? 0,
      loanId: Number(payment.loanId),
      scheduleId: Number(payment.scheduleId),
      amountPaid: Number(payment.amountPaid),
      paymentDate: this.toApiDate(payment.paymentDate),
      ...((payment.id ?? 0) > 0
        ? { transactionId: payment.transactionId?.trim() ?? '' }
        : {}),
      paymentMode: payment.paymentMode?.trim() || null,
      paymentStatus: payment.paymentStatus?.trim() || 'Success',
      remarks: payment.remarks?.trim() || null,
    };
  }

  private toApiDate(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? new Date().toISOString() : date.toISOString();
  }

  private normalizePayment(payment: any): LoanPayment {
    return {
      id: payment?.Id ?? payment?.id,
      loanId: payment?.LoanId ?? payment?.loanId ?? 0,
      scheduleId: payment?.ScheduleId ?? payment?.scheduleId ?? 0,
      amountPaid: payment?.AmountPaid ?? payment?.amountPaid ?? 0,
      paymentDate: payment?.PaymentDate ?? payment?.paymentDate ?? '',
      transactionId: payment?.TransactionId ?? payment?.transactionId ?? '',
      paymentMode: payment?.PaymentMode ?? payment?.paymentMode ?? null,
      paymentStatus: payment?.PaymentStatus ?? payment?.paymentStatus ?? 'Success',
      remarks: payment?.Remarks ?? payment?.remarks ?? null,
    };
  }

  private normalizeUnpaidInstallment(installment: any): UnpaidInstallment {
    return {
      id: installment?.ScheduleId ?? installment?.scheduleId ?? 0,
      loanId: installment?.LoanId ?? installment?.loanId ?? 0,
      installmentNo:
        installment?.InstallmentNo ?? installment?.installmentNo ?? 0,
      dueDate: installment?.DueDate ?? installment?.dueDate ?? '',
      emiAmount: installment?.EMIAmount ?? installment?.emiAmount ?? 0,
      principalAmount:
        installment?.PrincipalAmount ?? installment?.principalAmount ?? 0,
      interestAmount:
        installment?.InterestAmount ?? installment?.interestAmount ?? 0,
      outstandingBalance:
        installment?.OutstandingBalance ??
        installment?.outstandingBalance ??
        0,
    };
  }
}
