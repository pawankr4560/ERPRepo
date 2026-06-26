import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs';

import { environment } from '../../environments/environment';

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

export interface DashboardActiveLoan {
  loanId: number;
  loanNumber: string;
  customerName: string;
  loanAmount: number;
  rate: number;
  emi: number;
  tenureMonths: number;
  paidInstallments: number;
  totalInstallments: number;
  monthsRemaining: number;
  outstandingBalance: number;
  progressPercentage: number;
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
  creditScore: number;
  creditScoreChange: number;
  creditUtilization: number;
  averageLoanAgeYears: number;
  hardInquiries: number;
  paymentHistoryRating: string;
  upcomingInstallments: DashboardInstallment[];
  recentPayments: DashboardPayment[];
  activeLoanSummaries: DashboardActiveLoan[];
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
      .get<any>(`${environment.apiUrl}/Dashboard/loan-summary`, {
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
      creditScore: value?.creditScore ?? value?.CreditScore ?? 0,
      creditScoreChange:
        value?.creditScoreChange ?? value?.CreditScoreChange ?? 0,
      creditUtilization:
        value?.creditUtilization ?? value?.CreditUtilization ?? 0,
      averageLoanAgeYears:
        value?.averageLoanAgeYears ?? value?.AverageLoanAgeYears ?? 0,
      hardInquiries: value?.hardInquiries ?? value?.HardInquiries ?? 0,
      paymentHistoryRating:
        value?.paymentHistoryRating ?? value?.PaymentHistoryRating ?? 'No history',
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
      activeLoanSummaries: (
        value?.activeLoanSummaries ??
        value?.ActiveLoanSummaries ??
        []
      ).map((item: any) => ({
        loanId: item?.loanId ?? item?.LoanId ?? 0,
        loanNumber: item?.loanNumber ?? item?.LoanNumber ?? '',
        customerName: item?.customerName ?? item?.CustomerName ?? '',
        loanAmount: item?.loanAmount ?? item?.LoanAmount ?? 0,
        rate: item?.rate ?? item?.Rate ?? 0,
        emi: item?.emi ?? item?.Emi ?? item?.EMI ?? 0,
        tenureMonths: item?.tenureMonths ?? item?.TenureMonths ?? 0,
        paidInstallments:
          item?.paidInstallments ?? item?.PaidInstallments ?? 0,
        totalInstallments:
          item?.totalInstallments ?? item?.TotalInstallments ?? 0,
        monthsRemaining:
          item?.monthsRemaining ?? item?.MonthsRemaining ?? 0,
        outstandingBalance:
          item?.outstandingBalance ?? item?.OutstandingBalance ?? 0,
        progressPercentage:
          item?.progressPercentage ?? item?.ProgressPercentage ?? 0,
      })),
    };
  }
}
