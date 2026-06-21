import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute } from '@angular/router';
import { forkJoin, Subject, takeUntil } from 'rxjs';

import { ConfirmDialogComponent } from '../../../users/confirm-dialog-component/confirm-dialog-component';
import { Booking } from '../../booking/interfaces/booking';
import { BookingService } from '../../booking/services/booking-service';
import { BookingPayment } from '../interfaces/booking-payment';
import { BookingPaymentService } from '../services/booking-payment-service';

@Component({
  selector: 'app-booking-payment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTooltipModule,
  ],
  templateUrl: './booking-payment.html',
  styleUrl: './booking-payment.css',
})
export class BookingPaymentComponent implements OnInit, OnDestroy {
  readonly paymentMethods = ['Cash', 'Card', 'UPI', 'Bank Transfer', 'Cheque'];
  readonly paymentStatuses = ['Paid', 'Pending', 'Failed', 'Refunded'];
  readonly maxNotesLength = 500;

  bookings: Booking[] = [];
  payments: BookingPayment[] = [];
  current = this.emptyPayment();
  search = '';
  editing = false;
  isLoading = false;
  isSaving = false;
  selectedBookingId?: number;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private bookingService: BookingService,
    private paymentService: BookingPaymentService,
    private route: ActivatedRoute,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.bookingService.bookings$
      .pipe(takeUntil(this.destroy$))
      .subscribe((bookings) => (this.bookings = bookings));
    this.paymentService.payments$
      .pipe(takeUntil(this.destroy$))
      .subscribe((payments) => (this.payments = payments));

    const bookingId = Number(this.route.snapshot.queryParamMap.get('bookingId'));
    this.selectedBookingId = bookingId > 0 ? bookingId : undefined;
    this.refresh();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get filteredPayments(): BookingPayment[] {
    const query = this.search.trim().toLowerCase();
    if (!query) return this.payments;

    return this.payments.filter((payment) =>
      [
        payment.bookingNumber,
        payment.customerName,
        payment.transactionReference,
        payment.paymentMethod,
        payment.status,
        payment.notes,
        payment.amount,
      ].some((value) => String(value ?? '').toLowerCase().includes(query))
    );
  }

  get selectedBooking(): Booking | undefined {
    return this.bookings.find(
      (booking) => booking.id === Number(this.current.bookingId)
    );
  }

  get payableBookings(): Booking[] {
    return this.bookings.filter(
      (booking) =>
        booking.id === Number(this.current.id ? this.current.bookingId : 0) ||
        (booking.status.toLowerCase() !== 'cancelled' &&
          booking.paymentStatus.toLowerCase() !== 'paid')
    );
  }

  get outstandingAmount(): number {
    const booking = this.selectedBooking;
    if (!booking) return 0;

    const paid = this.payments
      .filter(
        (payment) =>
          payment.bookingId === booking.id &&
          payment.status === 'Paid' &&
          payment.id !== this.current.id
      )
      .reduce((total, payment) => total + Number(payment.amount), 0);

    return Math.max(0, Number(booking.amount) - paid);
  }

  refresh(): void {
    this.isLoading = true;
    forkJoin({
      bookings: this.bookingService.loadBookings(),
      payments: this.paymentService.loadPayments(this.selectedBookingId),
    }).subscribe({
      next: () => {
        this.isLoading = false;
        if (this.selectedBookingId && !this.editing) {
          this.startCreate(this.selectedBookingId);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.showError(error, 'Unable to load booking payments.');
      },
    });
  }

  startCreate(bookingId?: number): void {
    const targetBookingId = bookingId ?? this.selectedBookingId;
    const targetBooking = targetBookingId
      ? this.bookings.find((booking) => booking.id === targetBookingId)
      : undefined;

    if (targetBooking?.paymentStatus.toLowerCase() === 'paid') {
      this.editing = false;
      this.snackBar.open(
        `Booking ${targetBooking.bookingNumber} is already fully paid.`,
        'Close',
        { duration: 4500 }
      );
      return;
    }

    if (!targetBookingId && this.payableBookings.length === 0) {
      this.editing = false;
      this.snackBar.open(
        'All bookings are fully paid or cancelled. There is no outstanding payment to record.',
        'Close',
        { duration: 4500 }
      );
      return;
    }

    this.current = this.emptyPayment();
    this.current.bookingId = targetBookingId ?? 0;
    this.onBookingChange();
    this.editing = true;
  }

  startEdit(payment: BookingPayment): void {
    this.current = {
      ...payment,
      paymentDate: this.toDateTimeInput(payment.paymentDate),
    };
    this.editing = true;
  }

  cancel(): void {
    this.current = this.emptyPayment();
    this.editing = false;
  }

  onBookingChange(): void {
    if (!this.current.id) {
      this.current.amount = this.outstandingAmount;
    }
  }

  save(form: NgForm): void {
    form.control.markAllAsTouched();
    if (form.invalid || this.isSaving) return;

    if (!this.current.id && this.outstandingAmount <= 0) {
      this.snackBar.open(
        'This booking is already fully paid. No additional payment is due.',
        'Close',
        { duration: 4500, panelClass: ['error-snackbar'] }
      );
      return;
    }

    if (this.current.status === 'Paid' && this.current.amount > this.outstandingAmount) {
      this.snackBar.open(
        `Amount cannot exceed the outstanding balance of INR ${this.outstandingAmount.toFixed(2)}.`,
        'Close',
        { duration: 4500, panelClass: ['error-snackbar'] }
      );
      return;
    }

    this.isSaving = true;
    const request = this.current.id
      ? this.paymentService.updatePayment(this.current)
      : this.paymentService.createPayment(this.current);
    const action = this.current.id ? 'updated' : 'created';

    request.subscribe({
      next: () => {
        this.isSaving = false;
        this.snackBar.open(`Payment ${action} successfully.`, 'Close', {
          duration: 3000,
        });
        this.cancel();
        this.bookingService.loadBookings().subscribe();
      },
      error: (error) => {
        this.isSaving = false;
        this.showError(error, `Payment could not be ${action}.`);
      },
    });
  }

  remove(payment: BookingPayment): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Booking Payment',
        message: `Delete payment "${payment.transactionReference}"?`,
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      this.paymentService.deletePayment(payment.id).subscribe({
        next: () => {
          this.snackBar.open('Payment deleted successfully.', 'Close', {
            duration: 3000,
          });
          this.bookingService.loadBookings().subscribe();
        },
        error: (error) => this.showError(error, 'Unable to delete payment.'),
      });
    });
  }

  bookingLabel(bookingId: number): string {
    const booking = this.bookings.find((item) => item.id === Number(bookingId));
    return booking
      ? `${booking.bookingNumber} - ${booking.userName || booking.userId}`
      : `Booking ${bookingId}`;
  }

  statusClass(status: string): string {
    return `status-${(status || 'Pending').toLowerCase()}`;
  }

  private emptyPayment(): BookingPayment {
    return {
      id: 0,
      bookingId: 0,
      bookingNumber: '',
      customerName: '',
      bookingAmount: 0,
      amount: 0,
      paymentDate: this.toDateTimeInput(new Date().toISOString()),
      paymentMethod: '',
      status: 'Paid',
      transactionReference: '',
      notes: null,
      createdDate: '',
    };
  }

  private toDateTimeInput(value: string): string {
    const date = value ? new Date(value) : new Date();
    if (Number.isNaN(date.getTime())) return '';
    const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000);
    return local.toISOString().slice(0, 16);
  }

  private showError(error: any, fallback: string): void {
    const errors = error?.error?.errors;
    const validationMessage = errors
      ? (Object.values(errors).flat() as string[])[0]
      : null;
    this.snackBar.open(
      error?.error?.message ||
        error?.error?.errorMessage ||
        validationMessage ||
        fallback,
      'Close',
      { duration: 5000, panelClass: ['error-snackbar'] }
    );
  }
}
