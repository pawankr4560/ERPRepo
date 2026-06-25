import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { finalize, Subject, switchMap, takeUntil } from 'rxjs';
import { Router } from '@angular/router';

import { ConfirmDialogComponent } from '../../../users/confirm-dialog-component/confirm-dialog-component';
import { BookingDialog } from '../booking-dialog/booking-dialog';
import { Booking, BookingDialogMode, BookingUser } from '../interfaces/booking';
import { BookingService } from '../services/booking-service';
import { Car } from '../../car/interfaces/car';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-booking-master',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './booking-master.html',
  styleUrl: './booking-master.css',
})
export class BookingMaster implements OnInit, AfterViewInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  cars: Car[] = [];
  users: BookingUser[] = [];
  isLoading = false;
  isMutating = false;

  get isBusy(): boolean {
    return this.isLoading || this.isMutating;
  }

  displayedColumns = [
    'bookingNumber',
    'user',
    'car',
    'dates',
    'amount',
    'status',
    'paymentStatus',
    'actions',
  ];

  readonly dataSource = new MatTableDataSource<Booking>([]);
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private bookingService: BookingService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.breakpointObserver
      .observe(['(max-width: 768px)'])
      .pipe(takeUntil(this.destroy$))
      .subscribe(({ matches }) => {
        this.displayedColumns = matches
          ? ['bookingNumber', 'car', 'status', 'actions']
          : [
              'bookingNumber',
              'user',
              'car',
              'dates',
              'amount',
              'status',
              'paymentStatus',
              'actions',
            ];
      });

    this.bookingService.bookings$
      .pipe(takeUntil(this.destroy$))
      .subscribe((bookings) => {
        this.dataSource.data = [...bookings];
        this.dataSource.paginator = this.paginator;
      });

    this.loadData();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  createBooking(): void {
    this.isLoading = true;
    this.bookingService.loadDialogOptions()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
      next: ({ cars, users }) => {
        this.cars = cars;
        this.users = users;

        if (users.length === 0) {
          this.notify('No active users are available for booking.');
          return;
        }

        if (cars.length === 0) {
          this.notify(
            'No available cars. Add a car or change its status from Maintenance/Inactive in Car Master.'
          );
          return;
        }

        this.openDialog('create');
      },
      error: (error) => {
        this.notify(this.errorMessage(error, 'Unable to load booking options'));
      },
    });
  }

  managePayments(booking: Booking): void {
    this.router.navigate(['/home/booking/payments'], {
      queryParams: { bookingId: booking.id },
    });
  }

  viewBooking(booking: Booking): void {
    this.openDialog('view', booking);
  }

  editBooking(booking: Booking): void {
    this.openDialog('edit', booking);
  }

  deleteBooking(booking: Booking): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '390px',
      data: {
        title: 'Delete Booking',
        message: `Delete booking "${booking.bookingNumber}"? The car availability will be recalculated.`,
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      this.isMutating = true;
      this.bookingService.deleteBooking(booking.id)
        .pipe(finalize(() => (this.isMutating = false)))
        .subscribe({
        next: () => {
          this.toastService.success('Booking deleted successfully');
          this.notify('Booking deleted successfully');
          this.refreshOptions();
        },
        error: (error) => this.notify(this.errorMessage(error, 'Delete failed')),
      });
    });
  }

  private loadData(): void {
    this.isLoading = true;
    this.bookingService.loadDialogOptions().pipe(
      switchMap(({ cars, users }) => {
        this.cars = cars;
        this.users = users;
        return this.bookingService.loadBookings();
      }),
      finalize(() => (this.isLoading = false))
    ).subscribe({
      error: (error) => {
        this.notify(this.errorMessage(error, 'Unable to load booking data'));
      },
    });
  }

  private openDialog(mode: BookingDialogMode, booking?: Booking): void {
    const ref = this.dialog.open(BookingDialog, {
      width: '760px',
      maxWidth: '95vw',
      data: { mode, booking, cars: this.cars, users: this.users },
    });

    if (mode === 'view') return;

    ref.afterClosed().subscribe((result: Booking | null) => {
      if (!result) return;

      const request =
        mode === 'create'
          ? this.bookingService.createBooking(result)
          : this.bookingService.updateBooking(result);

      this.isMutating = true;
      request.pipe(finalize(() => (this.isMutating = false))).subscribe({
        next: () => {
          this.toastService.success(
            mode === 'create'
              ? 'Booking created successfully'
              : 'Booking updated successfully'
          );
          this.notify(
            mode === 'create'
              ? 'Booking created successfully'
              : 'Booking updated successfully'
          );
          this.refreshOptions();
        },
        error: (error) =>
          this.notify(
            this.errorMessage(
              error,
              mode === 'create' ? 'Create failed' : 'Update failed'
            )
          ),
      });
    });
  }

  private refreshOptions(): void {
    this.isLoading = true;
    this.bookingService.loadDialogOptions()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
      next: ({ cars, users }) => {
        this.cars = cars;
        this.users = users;
      },
      error: (error) =>
        this.notify(this.errorMessage(error, 'Unable to refresh booking options')),
    });
  }

  private notify(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3500,
      horizontalPosition: 'right',
      verticalPosition: 'top',
    });
  }

  private errorMessage(error: any, fallback: string): string {
    const validationErrors = error?.error?.errors;
    const firstValidationError = validationErrors
      ? (Object.values(validationErrors).flat() as string[])[0]
      : null;

    return (
      error?.error?.message ||
      error?.error?.errorMessage ||
      firstValidationError ||
      fallback
    );
  }
}
