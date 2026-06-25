import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { UnpaidInstallment } from '../services/loan-payment-service';
import {
  RazorpayService,
  UserLoanSummary,
} from '../services/razorpay-service';

@Component({
  selector: 'app-emi-pay',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
  ],
  templateUrl: './emi-pay.html',
  styleUrl: './emi-pay.css',
})
export class EmiPayComponent implements OnInit {
  loans: UserLoanSummary[] = [];
  installments: UnpaidInstallment[] = [];
  selectedLoanId = 0;
  selectedScheduleId = 0;
  isLoadingLoans = false;
  isLoadingInstallments = false;
  isPaying = false;
  razorpayReady = false;

  constructor(
    private razorpayService: RazorpayService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadLoans();
    this.razorpayService.getConfig().subscribe({
      next: (config) => {
        this.razorpayReady = !!config.keyId;
      },
      error: () => {
        this.razorpayReady = false;
      },
    });
  }

  get selectedInstallment(): UnpaidInstallment | undefined {
    return this.installments.find(
      (item) => item.id === Number(this.selectedScheduleId)
    );
  }

  get canPay(): boolean {
    return (
      this.razorpayReady &&
      !this.isPaying &&
      Number(this.selectedLoanId) > 0 &&
      Number(this.selectedScheduleId) > 0 &&
      !!this.selectedInstallment
    );
  }

  loadLoans(): void {
    this.isLoadingLoans = true;
    this.razorpayService
      .getMyLoans()
      .pipe(finalize(() => (this.isLoadingLoans = false)))
      .subscribe({
        next: (loans) => {
          this.loans = loans;
          if (!loans.length) {
            this.snackBar.open('No active loans found for your account.', 'Close', {
              duration: 4000,
            });
          }
        },
        error: (error) => this.showError('Unable to load your loans.', error),
      });
  }

  onLoanChange(): void {
    this.selectedScheduleId = 0;
    this.installments = [];

    const loanId = Number(this.selectedLoanId);
    if (!loanId) {
      return;
    }

    this.isLoadingInstallments = true;
    this.razorpayService
      .getUnpaidInstallments(loanId)
      .pipe(finalize(() => (this.isLoadingInstallments = false)))
      .subscribe({
        next: (installments) => {
          this.installments = installments;
          if (!installments.length) {
            this.snackBar.open('All installments are paid for this loan.', 'Close', {
              duration: 3500,
            });
          }
        },
        error: (error) =>
          this.showError('Unable to load unpaid installments.', error),
      });
  }

  payEmi(): void {
    const loanId = Number(this.selectedLoanId);
    const scheduleId = Number(this.selectedScheduleId);

    if (!this.canPay || !this.selectedInstallment) {
      return;
    }

    this.isPaying = true;
    this.razorpayService.createEmiOrder(loanId, scheduleId).subscribe({
      next: (order) => {
        this.razorpayService.openCheckout(
          order,
          loanId,
          scheduleId,
          () => {
            this.isPaying = false;
            this.snackBar.open('EMI paid successfully via Razorpay.', 'Close', {
              duration: 4000,
            });
            this.selectedScheduleId = 0;
            this.onLoanChange();
            this.loadLoans();
          },
          (message) => {
            this.isPaying = false;
            if (message !== 'Payment was cancelled.') {
              this.snackBar.open(message, 'Close', {
                duration: 5000,
                panelClass: ['error-snackbar'],
              });
            }
          }
        );
      },
      error: (error) => {
        this.isPaying = false;
        this.showError('Unable to start Razorpay checkout.', error);
      },
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-IN', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value);
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

  isDue(installment: UnpaidInstallment): boolean {
    const dueDate = new Date(installment.dueDate);
    if (Number.isNaN(dueDate.getTime())) {
      return false;
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);
    dueDate.setHours(0, 0, 0, 0);
    return today >= dueDate;
  }

  private showError(message: string, error: any): void {
    const apiMessage = error?.error?.message ?? error?.error?.Message;
    this.snackBar.open(apiMessage || message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar'],
    });
  }
}
