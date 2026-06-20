import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
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
    MatIconModule,
    MatRadioModule,
    MatSnackBarModule,
  ],
  templateUrl: './setting.html',
  styleUrl: './setting.css',
})
export class Setting implements OnInit {
  interestCalculationType: InterestCalculationType = 'Reducing';
  savedInterestCalculationType: InterestCalculationType = 'Reducing';
  isLoading = true;
  isSaving = false;

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
}
