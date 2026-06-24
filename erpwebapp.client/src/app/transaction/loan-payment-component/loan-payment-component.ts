import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { finalize, forkJoin, Subject, takeUntil } from 'rxjs';
import { ConfirmDialogComponent } from '../../users/confirm-dialog-component/confirm-dialog-component';
import {
  Loan,
  LoanEMISchedule,
  LoanPayment,
  LoanService,
} from '../services/loan-service';
import {
  LoanPaymentService,
  UnpaidInstallment,
} from '../services/loan-payment-service';

@Component({
  selector: 'app-loan-payment-component',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './loan-payment-component.html',
  styleUrl: './loan-payment-component.css',
})
export class LoanPaymentComponent implements OnInit, OnDestroy {
  payments: LoanPayment[] = [];
  loans: Loan[] = [];
  search = '';
  editing = false;
  isLoading = false;
  isSaving = false;
  isLoadingInstallments = false;
  isLoadingPayment = false;
  isDeleting = false;
  unpaidInstallments: UnpaidInstallment[] = [];

  readonly paymentModes = ['Cash', 'Bank Transfer', 'UPI', 'Card', 'Cheque'];
  readonly paymentStatuses = ['Success', 'Pending', 'Failed', 'Refunded'];
  readonly maxRemarksLength = 500;

  current: LoanPayment = this.createEmptyPayment();
  private readonly destroyed$ = new Subject<void>();

  get isBusy(): boolean {
    return this.isLoading || this.isSaving || this.isLoadingInstallments ||
      this.isLoadingPayment || this.isDeleting;
  }

  constructor(
    private paymentService: LoanPaymentService,
    private loanService: LoanService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.paymentService.payments$
      .pipe(takeUntil(this.destroyed$))
      .subscribe((payments) => (this.payments = payments));
    this.loanService.loans$
      .pipe(takeUntil(this.destroyed$))
      .subscribe((loans) => (this.loans = loans ?? []));
    this.refresh();
  }

  ngOnDestroy(): void {
    this.destroyed$.next();
    this.destroyed$.complete();
  }

  get filteredPayments(): LoanPayment[] {
    const query = this.search.trim().toLowerCase();
    if (!query) {
      return this.payments;
    }

    return this.payments.filter((payment) => {
      const loanNumber = this.getLoanNumber(payment.loanId).toLowerCase();
      const schedule = this.getScheduleLabel(payment).toLowerCase();
      return [
        loanNumber,
        schedule,
        payment.transactionId,
        payment.paymentMode,
        payment.paymentStatus,
        payment.remarks,
        payment.amountPaid,
      ].some((value) => String(value ?? '').toLowerCase().includes(query));
    });
  }

  get availableSchedules(): LoanEMISchedule[] {
    if (!this.current.id) {
      return this.unpaidInstallments.map((installment) => ({
        ...installment,
        isPaid: false,
      }));
    }

    const loan = this.loans.find((item) => item.id === Number(this.current.loanId));
    const schedules = loan?.emiSchedules ?? [];

    return schedules
      .filter(
        (schedule) =>
          !schedule.isPaid ||
          (this.current.id != null && schedule.id === Number(this.current.scheduleId))
      )
      .sort((a, b) => a.installmentNo - b.installmentNo);
  }

  get selectedInstallment(): LoanEMISchedule | undefined {
    return this.availableSchedules.find(
      (schedule) => schedule.id === Number(this.current.scheduleId)
    );
  }

  get selectedDueDateInput(): string | null {
    const datePart = this.selectedInstallment?.dueDate?.slice(0, 10);
    return datePart && /^\d{4}-\d{2}-\d{2}$/.test(datePart)
      ? `${datePart}T00:00`
      : null;
  }

  get isPaymentBeforeDueDate(): boolean {
    if (this.current.id || !this.current.paymentDate || !this.selectedInstallment) {
      return false;
    }

    const paymentDate = this.toCalendarDate(this.current.paymentDate);
    const dueDate = this.toCalendarDate(this.selectedInstallment.dueDate);
    return !!paymentDate && !!dueDate && paymentDate < dueDate;
  }

  refresh(): void {
    this.isLoading = true;
    forkJoin({
      loans: this.loanService.loadLoans(),
      payments: this.paymentService.loadPayments(),
    }).pipe(finalize(() => (this.isLoading = false))).subscribe({
      error: (error) => {
        this.showError('Unable to load loans and payments.', error);
      },
    });
  }

  startCreate(): void {
    this.current = this.createEmptyPayment();
    this.unpaidInstallments = [];
    this.editing = true;
  }

  startEdit(payment: LoanPayment): void {
    if (payment.id == null) {
      return;
    }

    this.isLoadingPayment = true;
    this.paymentService.getPayment(payment.id)
      .pipe(finalize(() => (this.isLoadingPayment = false)))
      .subscribe({
      next: (result) => {
        this.current = {
          ...result,
          paymentDate: this.toDateTimeInput(result.paymentDate),
        };
        this.editing = true;
      },
      error: (error) => this.showError('Unable to load the payment.', error),
    });
  }

  cancel(): void {
    this.current = this.createEmptyPayment();
    this.unpaidInstallments = [];
    this.editing = false;
  }

  onLoanChange(): void {
    this.current.scheduleId = 0;
    this.current.amountPaid = 0;
    this.unpaidInstallments = [];

    const loan = this.loans.find(
      (item) => item.id === Number(this.current.loanId)
    );
    if (!loan?.loanNumber) {
      return;
    }

    this.isLoadingInstallments = true;
    this.paymentService.getUnpaidInstallments(loan.loanNumber).subscribe({
      next: (installments) => {
        this.unpaidInstallments = installments;
        this.isLoadingInstallments = false;
        if (!installments.length) {
          this.snackBar.open('This loan has no unpaid installments.', 'Close', {
            duration: 3500,
          });
        }
      },
      error: (error) => {
        this.isLoadingInstallments = false;
        this.showError('Unable to load unpaid installments.', error);
      },
    });
  }

  onScheduleChange(scheduleId: number): void {
    const schedule = this.availableSchedules.find(
      (item) => item.id === Number(scheduleId)
    );
    if (schedule) {
      this.current.amountPaid = schedule.emiAmount;
    }
  }

  save(form: NgForm): void {
    form.control.markAllAsTouched();
    if (this.isPaymentBeforeDueDate) {
      this.snackBar.open(
        'Payment cannot be created before the installment due date.',
        'Close',
        { duration: 4500, panelClass: ['error-snackbar'] }
      );
      return;
    }

    if (form.invalid || this.isSaving || !this.isValidPayment()) {
      return;
    }

    this.isSaving = true;
    const request$ = this.current.id
      ? this.paymentService.updatePayment(this.current)
      : this.paymentService.createPayment(this.current);
    const action = this.current.id ? 'updated' : 'created';

    request$.subscribe({
      next: () => {
        this.isSaving = false;
        this.snackBar.open(`Payment ${action} successfully.`, 'Close', {
          duration: 3000,
        });
        this.cancel();
        this.refresh();
      },
      error: (error) => {
        this.isSaving = false;
        this.showError(`Payment could not be ${action}.`, error);
      },
    });
  }

  remove(payment: LoanPayment): void {
    if (payment.id == null) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Payment',
        message: `Delete transaction "${payment.transactionId}"? This action cannot be undone.`,
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.isDeleting = true;
      this.paymentService.deletePayment(payment.id as number)
        .pipe(finalize(() => (this.isDeleting = false)))
        .subscribe({
        next: () => {
          this.snackBar.open('Payment deleted successfully.', 'Close', {
            duration: 3000,
          });
          this.refresh();
        },
        error: (error) => this.showError('Unable to delete the payment.', error),
      });
    });
  }

  clearSearch(): void {
    this.search = '';
  }

  getLoanNumber(loanId: number): string {
    return this.loans.find((loan) => loan.id === Number(loanId))?.loanNumber ?? `Loan ${loanId}`;
  }

  getCustomerName(loanId: number): string {
    const loan = this.loans.find((item) => item.id === Number(loanId));
    return loan?.userName || (loan?.userId ? String(loan.userId) : '-');
  }

  getScheduleLabel(payment: LoanPayment): string {
    const loan = this.loans.find((item) => item.id === Number(payment.loanId));
    const schedule = loan?.emiSchedules?.find(
      (item) => item.id === Number(payment.scheduleId)
    );
    return schedule ? `Installment ${schedule.installmentNo}` : `Schedule ${payment.scheduleId}`;
  }

  getStatusClass(status: string): string {
    return `status-${(status || 'Pending').toLowerCase()}`;
  }

  private isValidPayment(): boolean {
    return (
      Number(this.current.loanId) > 0 &&
      Number(this.current.scheduleId) > 0 &&
      Number(this.current.amountPaid) > 0 &&
      !!this.current.paymentDate &&
      !!this.current.paymentMode?.trim() &&
      !!this.current.paymentStatus?.trim() &&
      !this.isPaymentBeforeDueDate &&
      (this.current.remarks?.length ?? 0) <= this.maxRemarksLength
    );
  }

  private createEmptyPayment(): LoanPayment {
    return {
      id: 0,
      loanId: 0,
      scheduleId: 0,
      amountPaid: 0,
      paymentDate: this.toDateTimeInput(new Date().toISOString()),
      transactionId: '',
      paymentMode: null,
      paymentStatus: 'Success',
      remarks: null,
    };
  }

  private toDateTimeInput(value: string): string {
    const date = value ? new Date(value) : new Date();
    if (Number.isNaN(date.getTime())) {
      return '';
    }

    const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
    return local.toISOString().slice(0, 16);
  }

  private toCalendarDate(value: string): string | null {
    const match = value?.match(/^(\d{4})-(\d{2})-(\d{2})/);
    return match ? `${match[1]}-${match[2]}-${match[3]}` : null;
  }

  private showError(message: string, error: any): void {
    const apiMessage =
      error?.error?.errorMessage ??
      error?.error?.message ??
      this.firstValidationError(error?.error?.errors);
    this.snackBar.open(apiMessage || message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar'],
    });
  }

  private firstValidationError(errors: Record<string, string[]> | undefined): string | null {
    if (!errors) {
      return null;
    }

    const first = Object.values(errors).flat()[0];
    return first ?? null;
  }
}
