import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { Car, CarDialogData } from '../interfaces/car';

@Component({
  selector: 'app-car-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './car-dialog.html',
  styleUrl: './car-dialog.css',
})
export class CarDialog {
  readonly isViewMode: boolean;
  readonly title: string;
  readonly form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CarDialog>,
    @Inject(MAT_DIALOG_DATA) public data: CarDialogData
  ) {
    this.isViewMode = data.mode === 'view';
    this.title =
      data.mode === 'create'
        ? 'Add Car'
        : data.mode === 'edit'
          ? 'Edit Car'
          : 'Car Details';

    this.form = this.fb.group({
      id: [data.car?.id ?? 0],
      brand: [data.car?.brand ?? '', [Validators.required, Validators.maxLength(50)]],
      model: [data.car?.model ?? '', [Validators.required, Validators.maxLength(50)]],
      year: [
        data.car?.year ?? new Date().getFullYear(),
        [Validators.required, Validators.min(1900), Validators.max(new Date().getFullYear() + 1)],
      ],
      categoryId: [data.car?.categoryId ?? 0, [Validators.required, Validators.min(1)]],
      transmission: [data.car?.transmission ?? '', Validators.required],
      fuelType: [data.car?.fuelType ?? '', Validators.required],
      seats: [data.car?.seats ?? 5, [Validators.required, Validators.min(1), Validators.max(100)]],
      pricePerDay: [data.car?.pricePerDay ?? 0, [Validators.required, Validators.min(1)]],
      imageUrl: [
        data.car?.imageUrl ?? '',
        [
          Validators.maxLength(500),
          Validators.pattern(/^https?:\/\/.+/i),
        ],
      ],
      status: [data.car?.status ?? 'Available', Validators.required],
    });

    if (this.isViewMode) {
      this.form.disable();
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(this.form.getRawValue() as Car);
  }

  close(): void {
    this.dialogRef.close(null);
  }
}
