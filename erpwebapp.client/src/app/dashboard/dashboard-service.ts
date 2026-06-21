import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs';

import { environment } from '../environments/environment';

export interface DashboardInstallment {
  id: number;
  loanId: number;
  loanNumber: string;
  customerName: string;
  installmentNo: number;
  dueDate: string;
  emiAmount: number;
}

export interface DashboardPayment {
  id: number;
  loanId: number;
  loanNumber: string;
  customerName: string;
  amountPaid: number;
  paymentDate: string;
  paymentStatus: string;
  transactionId: string;
}

export interface LoanDashboardSummary {
  totalPortfolio: number;
  totalCollected: number;
  outstandingPortfolio: number;
  overdueAmount: number;
  totalLoans: number;
  activeLoans: number;
  pendingLoans: number;
  completedLoans: number;
  otherLoans: number;
  overdueInstallmentCount: number;
  collectionRate: number;
  queryDurationMs: number;
  generatedAtUtc: string;
  upcomingInstallments: DashboardInstallment[];
  recentPayments: DashboardPayment[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly headers = new HttpHeaders({
    'Content-Type': 'application/json; charset=utf-8',
    api_key: environment.apiKey,
  });

  constructor(private http: HttpClient) {}

  loadSummary() {
    return this.http
      .get<any>(`${environment.apiUrl}/api/Dashboard/loan-summary`, {
        headers: this.headers,
      })
      .pipe(map((response) => this.normalize(response?.data ?? response)));
  }

  private normalize(value: any): LoanDashboardSummary {
    return {
      totalPortfolio: value?.totalPortfolio ?? value?.TotalPortfolio ?? 0,
      totalCollected: value?.totalCollected ?? value?.TotalCollected ?? 0,
      outstandingPortfolio:
        value?.outstandingPortfolio ?? value?.OutstandingPortfolio ?? 0,
      overdueAmount: value?.overdueAmount ?? value?.OverdueAmount ?? 0,
      totalLoans: value?.totalLoans ?? value?.TotalLoans ?? 0,
      activeLoans: value?.activeLoans ?? value?.ActiveLoans ?? 0,
      pendingLoans: value?.pendingLoans ?? value?.PendingLoans ?? 0,
      completedLoans: value?.completedLoans ?? value?.CompletedLoans ?? 0,
      otherLoans: value?.otherLoans ?? value?.OtherLoans ?? 0,
      overdueInstallmentCount:
        value?.overdueInstallmentCount ??
        value?.OverdueInstallmentCount ??
        0,
      collectionRate: value?.collectionRate ?? value?.CollectionRate ?? 0,
      queryDurationMs: value?.queryDurationMs ?? value?.QueryDurationMs ?? 0,
      generatedAtUtc: value?.generatedAtUtc ?? value?.GeneratedAtUtc ?? '',
      upcomingInstallments: (
        value?.upcomingInstallments ??
        value?.UpcomingInstallments ??
        []
      ).map((item: any) => ({
        id: item?.id ?? item?.Id ?? 0,
        loanId: item?.loanId ?? item?.LoanId ?? 0,
        loanNumber: item?.loanNumber ?? item?.LoanNumber ?? '',
        customerName: item?.customerName ?? item?.CustomerName ?? '',
        installmentNo: item?.installmentNo ?? item?.InstallmentNo ?? 0,
        dueDate: item?.dueDate ?? item?.DueDate ?? '',
        emiAmount: item?.emiAmount ?? item?.EmiAmount ?? 0,
      })),
      recentPayments: (
        value?.recentPayments ??
        value?.RecentPayments ??
        []
      ).map((item: any) => ({
        id: item?.id ?? item?.Id ?? 0,
        loanId: item?.loanId ?? item?.LoanId ?? 0,
        loanNumber: item?.loanNumber ?? item?.LoanNumber ?? '',
        customerName: item?.customerName ?? item?.CustomerName ?? '',
        amountPaid: item?.amountPaid ?? item?.AmountPaid ?? 0,
        paymentDate: item?.paymentDate ?? item?.PaymentDate ?? '',
        paymentStatus: item?.paymentStatus ?? item?.PaymentStatus ?? '',
        transactionId: item?.transactionId ?? item?.TransactionId ?? '',
      })),
    };
  }
}
