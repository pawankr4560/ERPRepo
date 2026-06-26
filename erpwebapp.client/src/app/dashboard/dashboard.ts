import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { finalize, Subject, takeUntil } from 'rxjs';

import {
  DashboardActiveLoan,
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
    FormsModule,
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
  selectedTheme: 'dark' | 'light' | 'classic' = 'dark';

  // Interactive Loan Calculator State
  loanAmount: number = 1390000;
  interestRate: number = 18.0;
  tenureMonths: number = 186;
  processingFee: number = 5000;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private dashboardService: DashboardService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('dashboardTheme') as 'dark' | 'light' | 'classic' | null;
    if (savedTheme === 'dark' || savedTheme === 'light' || savedTheme === 'classic') {
      this.selectedTheme = savedTheme;
    }

    this.refresh();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Current Date formatted as: "Friday, 27 June 2026"
  get formattedDate(): string {
    const d = new Date();
    const weekday = d.toLocaleDateString('en-US', { weekday: 'long' });
    const day = d.getDate();
    const month = d.toLocaleDateString('en-US', { month: 'long' });
    const year = d.getFullYear();
    return `${weekday}, ${day} ${month} ${year}`;
  }

  // Interactive Loan Calculator Getters
  get calculatedMonthlyEmi(): number {
    const P = this.loanAmount;
    const r = (this.interestRate / 100) / 12;
    const N = this.tenureMonths;
    if (r === 0) return Math.round(P / N);
    const emi = P * r * Math.pow(1 + r, N) / (Math.pow(1 + r, N) - 1);
    return Math.round(emi);
  }

  get calculatedTotalPayable(): number {
    // Run simulation to get exact total payable including last month's adjustment
    return this.runAmortizationSimulation().totalPayable;
  }

  get calculatedTotalInterest(): number {
    return this.runAmortizationSimulation().totalInterest;
  }

  get calculatedAmortizationSchedule(): any[] {
    return this.runAmortizationSimulation().schedule.slice(0, 5);
  }

  private runAmortizationSimulation() {
    const P = this.loanAmount;
    const r = (this.interestRate / 100) / 12;
    const N = this.tenureMonths;
    const emi = this.calculatedMonthlyEmi;

    let balance = P;
    let totalInterestPaid = 0;
    let totalPayableAmount = 0;
    const schedule = [];

    for (let i = 1; i <= N; i++) {
      const interest = Math.round(balance * r);
      let principal = emi - interest;

      if (i === N || balance - principal < 0) {
        principal = balance;
        const lastEmi = principal + interest;
        totalInterestPaid += interest;
        totalPayableAmount += lastEmi;
        schedule.push({
          emiNumber: i,
          principal: Math.round(principal),
          interest: Math.round(interest),
          balance: 0
        });
        break;
      }

      balance -= principal;
      totalInterestPaid += interest;
      totalPayableAmount += emi;
      schedule.push({
        emiNumber: i,
        principal: Math.round(principal),
        interest: Math.round(interest),
        balance: Math.round(balance)
      });
    }

    return {
      totalInterest: totalInterestPaid,
      totalPayable: totalPayableAmount,
      schedule
    };
  }

  // Fallbacks or properties mapping from Backend summary
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

  get activeLoanSummaries(): DashboardActiveLoan[] {
    return this.summary.activeLoanSummaries;
  }

  get nextEmiDue(): DashboardInstallment | undefined {
    return this.upcomingInstallments[0];
  }

  get nextEmiAmount(): number {
    return this.nextEmiDue?.emiAmount ?? 0;
  }

  get nextEmiDueText(): string {
    if (!this.nextEmiDue) {
      return 'No upcoming dues';
    }

    const dueDate = new Date(this.nextEmiDue.dueDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    dueDate.setHours(0, 0, 0, 0);
    const days = Math.ceil((dueDate.getTime() - today.getTime()) / 86400000);

    if (days < 0) return `${Math.abs(days)} days overdue`;
    if (days === 0) return 'Due today';
    if (days === 1) return 'Due tomorrow';
    return `in ${days} days`;
  }

  get interestPaidThisYear(): number {
    return this.summary.recentPayments
      .filter((payment) => new Date(payment.paymentDate).getFullYear() === new Date().getFullYear())
      .reduce((total, payment) => total + payment.amountPaid, 0);
  }

  get creditScore(): number {
    return this.summary.creditScore || 0;
  }

  get creditScoreChange(): number {
    return this.summary.creditScoreChange || 0;
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

  setTheme(theme: 'dark' | 'light' | 'classic'): void {
    this.selectedTheme = theme;
    localStorage.setItem('dashboardTheme', theme);
  }

  cycleTheme(): void {
    const nextTheme =
      this.selectedTheme === 'dark'
        ? 'light'
        : this.selectedTheme === 'light'
          ? 'classic'
          : 'dark';

    this.setTheme(nextTheme);
  }

  get themeLabel(): string {
    return `${this.selectedTheme[0].toUpperCase()}${this.selectedTheme.slice(1)}`;
  }

  createUser(): void {
    this.router.navigate(['/auth/signup']);
  }

  navigate(path: 'loans' | 'payments' | 'menu'): void {
    const routes = {
      loans: '/home/inventory/transactions',
      payments: '/home/inventory/payments',
      menu: '/home/menu',
    };

    this.router.navigate([routes[path]]);
  }

  paymentStatusClass(status: string): string {
    return `status-${(status || 'pending').toLowerCase()}`;
  }

  activeLoanIcon(loan: DashboardActiveLoan): string {
    const text = `${loan.loanNumber} ${loan.customerName}`.toLowerCase();
    if (text.includes('home')) return 'home';
    if (text.includes('car') || text.includes('vehicle')) return 'directions_car';
    if (text.includes('education') || text.includes('study')) return 'school';
    if (text.includes('personal')) return 'person';
    return 'account_balance';
  }

  activeLoanTone(index: number): string {
    return ['bg-blue', 'bg-green', 'bg-purple'][index % 3];
  }

  activeLoanProgressTone(index: number): string {
    return ['bg-blue', 'bg-orange', 'bg-blue'][index % 3];
  }

  formatDate(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? '-'
      : date.toLocaleDateString('en-GB', {
          day: '2-digit',
          month: 'short',
          year: 'numeric',
        });
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
      creditScore: 0,
      creditScoreChange: 0,
      creditUtilization: 0,
      averageLoanAgeYears: 0,
      hardInquiries: 0,
      paymentHistoryRating: 'No history',
      upcomingInstallments: [],
      recentPayments: [],
      activeLoanSummaries: [],
    };
  }
}
