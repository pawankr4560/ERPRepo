import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { finalize, Subject, takeUntil } from 'rxjs';

import {
  DashboardInstallment,
  DashboardPayment,
  DashboardService,
  LoanDashboardSummary,
} from './dashboard-service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit, OnDestroy {
  summary: LoanDashboardSummary = this.emptySummary();
  isLoading = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private dashboardService: DashboardService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.refresh();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get totalPortfolio(): number {
    return this.summary.totalPortfolio;
  }

  get totalCollected(): number {
    return this.summary.totalCollected;
  }

  get outstandingPortfolio(): number {
    return this.summary.outstandingPortfolio;
  }

  get activeLoans(): number {
    return this.summary.activeLoans;
  }

  get pendingLoans(): number {
    return this.summary.pendingLoans;
  }

  get completedLoans(): number {
    return this.summary.completedLoans;
  }

  get collectionRate(): number {
    return this.summary.collectionRate;
  }

  get overdueAmount(): number {
    return this.summary.overdueAmount;
  }

  get overdueInstallmentCount(): number {
    return this.summary.overdueInstallmentCount;
  }

  get upcomingInstallments(): DashboardInstallment[] {
    return this.summary.upcomingInstallments;
  }

  get recentPayments(): DashboardPayment[] {
    return this.summary.recentPayments;
  }

  get loanStatusBreakdown() {
    const statuses = [
      { label: 'Active', value: this.activeLoans, color: '#2563eb' },
      { label: 'Pending', value: this.pendingLoans, color: '#f59e0b' },
      { label: 'Completed', value: this.completedLoans, color: '#10b981' },
      { label: 'Other', value: this.summary.otherLoans, color: '#94a3b8' },
    ];

    return statuses.map((status) => ({
      ...status,
      percentage: this.summary.totalLoans
        ? (status.value / this.summary.totalLoans) * 100
        : 0,
    }));
  }

  refresh(): void {
    this.isLoading = true;
    this.dashboardService
      .loadSummary()
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => (this.isLoading = false))
      )
      .subscribe({
        next: (summary) => {
          this.summary = summary;
        },
        error: (error) => {
          const message =
            error?.error?.errorMessage ||
            error?.error?.message ||
            'Unable to load loan dashboard.';
          this.snackBar.open(message, 'Close', {
            duration: 4500,
            panelClass: ['error-snackbar'],
          });
        },
      });
  }

  createUser(): void {
    this.router.navigate(['/auth/signup']);
  }

  navigate(path: 'loans' | 'payments'): void {
    this.router.navigate([
      path === 'loans'
        ? '/home/inventory/transactions'
        : '/home/inventory/payments',
    ]);
  }

  paymentStatusClass(status: string): string {
    return `status-${(status || 'pending').toLowerCase()}`;
  }

  private emptySummary(): LoanDashboardSummary {
    return {
      totalPortfolio: 0,
      totalCollected: 0,
      outstandingPortfolio: 0,
      overdueAmount: 0,
      totalLoans: 0,
      activeLoans: 0,
      pendingLoans: 0,
      completedLoans: 0,
      otherLoans: 0,
      overdueInstallmentCount: 0,
      collectionRate: 0,
      queryDurationMs: 0,
      generatedAtUtc: '',
      upcomingInstallments: [],
      recentPayments: [],
    };
  }
}
