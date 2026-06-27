import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import {
  InterestCalculationType,
  InterestSettingService,
} from './interest-setting.service';

@Component({
  selector: 'app-setting',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatRadioModule,
    MatSnackBarModule,
  ],
  templateUrl: './setting.html',
  styleUrl: './setting.css',
})
export class Setting implements OnInit {
  interestCalculationType: InterestCalculationType = 'Reducing';
  savedInterestCalculationType: InterestCalculationType = 'Reducing';
  fixedCharge = 0;
  percentageCharge = 0;
  savedFixedCharge = 0;
  savedPercentageCharge = 0;
  isLoading = true;
  isSaving = false;
  isSavingCharges = false;

  constructor(
    private interestSettingService: InterestSettingService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.interestSettingService.load().subscribe((type) => {
      this.interestCalculationType = type;
      this.savedInterestCalculationType = type;
      this.isLoading = false;
    });

    this.interestSettingService.loadBookingPaymentCharges().subscribe((setting) => {
      this.fixedCharge = setting.fixedCharge;
      this.percentageCharge = setting.percentageCharge;
      this.savedFixedCharge = setting.fixedCharge;
      this.savedPercentageCharge = setting.percentageCharge;
    });
  }

  save(): void {
    if (this.isSaving) {
      return;
    }

    this.isSaving = true;
    this.interestSettingService.save(this.interestCalculationType).subscribe({
      next: (type) => {
        this.savedInterestCalculationType = type;
        this.interestCalculationType = type;
        this.isSaving = false;
        this.snackBar.open('Interest calculation setting saved.', 'Close', {
          duration: 3000,
        });
      },
      error: (error) => {
        this.isSaving = false;
        this.snackBar.open(
          error?.error?.errorMessage || 'Unable to save the interest setting.',
          'Close',
          { duration: 4000, panelClass: ['error-snackbar'] }
        );
      },
    });
  }

  saveBookingCharges(): void {
    if (this.isSavingCharges) {
      return;
    }

    this.isSavingCharges = true;
    this.interestSettingService
      .saveBookingPaymentCharges({
        fixedCharge: this.fixedCharge,
        percentageCharge: this.percentageCharge,
      })
      .subscribe({
        next: (setting) => {
          this.fixedCharge = setting.fixedCharge;
          this.percentageCharge = setting.percentageCharge;
          this.savedFixedCharge = setting.fixedCharge;
          this.savedPercentageCharge = setting.percentageCharge;
          this.isSavingCharges = false;
          this.snackBar.open('Booking checkout charges saved.', 'Close', {
            duration: 3000,
          });
        },
        error: (error) => {
          this.isSavingCharges = false;
          this.snackBar.open(
            error?.error?.errorMessage || error?.error?.message || 'Unable to save booking charges.',
            'Close',
            { duration: 4000, panelClass: ['error-snackbar'] }
          );
        },
      });
  }

  get chargesChanged(): boolean {
    return (
      Number(this.fixedCharge) !== Number(this.savedFixedCharge) ||
      Number(this.percentageCharge) !== Number(this.savedPercentageCharge)
    );
  }
}
