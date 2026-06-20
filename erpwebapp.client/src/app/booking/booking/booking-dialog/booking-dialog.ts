import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { Booking, BookingDialogData } from '../interfaces/booking';

@Component({
  selector: 'app-booking-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './booking-dialog.html',
  styleUrl: './booking-dialog.css',
})
export class BookingDialog {
  readonly isCreateMode: boolean;
  readonly isViewMode: boolean;
  readonly title: string;
  readonly minDate = this.toDateInput(new Date());
  readonly form: FormGroup;

  get selectableCars() {
    return this.data.cars.filter(
      (car) =>
        !['Maintenance', 'Inactive'].includes(car.status) ||
        car.id === this.data.booking?.carId
    );
  }

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<BookingDialog>,
    @Inject(MAT_DIALOG_DATA) public data: BookingDialogData
  ) {
    this.isCreateMode = data.mode === 'create';
    this.isViewMode = data.mode === 'view';
    this.title =
      data.mode === 'create'
        ? 'Create Booking'
        : data.mode === 'edit'
          ? 'Edit Booking'
          : 'Booking Details';

    const pickup = data.booking?.pickupDate
      ? this.toDateInput(new Date(data.booking.pickupDate))
      : this.minDate;
    const returnDate = data.booking?.returnDate
      ? this.toDateInput(new Date(data.booking.returnDate))
      : this.toDateInput(new Date(Date.now() + 24 * 60 * 60 * 1000));

    this.form = this.fb.group(
      {
        id: [data.booking?.id ?? 0],
        bookingNumber: [data.booking?.bookingNumber ?? ''],
        userId: [data.booking?.userId ?? '', Validators.required],
        carId: [data.booking?.carId ?? 0, [Validators.required, Validators.min(1)]],
        pickupDate: [pickup, Validators.required],
        returnDate: [returnDate, Validators.required],
        status: [data.booking?.status ?? 'Pending', Validators.required],
        paymentStatus: [
          data.booking?.paymentStatus ?? 'Pending',
          Validators.required,
        ],
        totalDays: [data.booking?.totalDays ?? 1],
        amount: [data.booking?.amount ?? 0],
        userName: [data.booking?.userName ?? ''],
        carName: [data.booking?.carName ?? ''],
        createdDate: [data.booking?.createdDate ?? new Date().toISOString()],
      },
      { validators: this.dateRangeValidator() }
    );

    if (this.isViewMode) {
      this.form.disable();
    }
  }

  get estimatedDays(): number {
    const pickup = new Date(this.form.get('pickupDate')?.value);
    const returned = new Date(this.form.get('returnDate')?.value);
    if (Number.isNaN(pickup.getTime()) || Number.isNaN(returned.getTime())) {
      return 0;
    }

    return Math.max(
      0,
      Math.ceil((returned.getTime() - pickup.getTime()) / 86_400_000)
    );
  }

  get estimatedAmount(): number {
    const carId = Number(this.form.get('carId')?.value);
    const price = this.data.cars.find((car) => car.id === carId)?.pricePerDay ?? 0;
    return this.estimatedDays * price;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.dialogRef.close({
      ...value,
      carId: Number(value.carId),
      pickupDate: `${value.pickupDate}T00:00:00`,
      returnDate: `${value.returnDate}T00:00:00`,
      totalDays: this.estimatedDays,
      amount: this.estimatedAmount,
    } as Booking);
  }

  close(): void {
    this.dialogRef.close(null);
  }

  private dateRangeValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const pickupValue = control.get('pickupDate')?.value;
      const returnValue = control.get('returnDate')?.value;
      if (!pickupValue || !returnValue) return null;

      const pickup = new Date(`${pickupValue}T00:00:00`);
      const returned = new Date(`${returnValue}T00:00:00`);
      const days = Math.ceil(
        (returned.getTime() - pickup.getTime()) / 86_400_000
      );

      if (returned <= pickup) return { returnBeforePickup: true };
      if (days > 365) return { bookingTooLong: true };
      if (this.isCreateMode && pickupValue < this.minDate) {
        return { pickupInPast: true };
      }

      return null;
    };
  }

  private toDateInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
