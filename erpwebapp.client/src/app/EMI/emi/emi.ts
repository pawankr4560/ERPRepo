import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  InterestCalculationType,
  InterestSettingService,
} from '../../setting/interest-setting.service';

@Component({
  selector: 'app-emi',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatSelectModule
  ],
  templateUrl: './emi.html',
  styleUrls: ['./emi.css'],
})
export class Emi implements OnInit {
 loanAmount: number = 2500000;
  tenure: number = 7;
  interestRate: number = 7.75;

  emi: number = 0;
  totalInterest: number = 0;
  totalPayable: number = 0;
  interestCalculationType: InterestCalculationType = 'Reducing';

  constructor(private interestSettingService: InterestSettingService) {}

  ngOnInit(): void {
    this.interestSettingService.load().subscribe((type) => {
      this.interestCalculationType = type;
      this.calculateEMI();
    });
  }

  calculateEMI(): void {
    const principal = this.loanAmount;
    const totalMonths = this.tenure * 12;
    this.emi = this.interestSettingService.calculateEmi(
      principal,
      this.interestRate,
      totalMonths,
      this.interestCalculationType
    );

    this.totalPayable = this.emi * totalMonths;
    this.totalInterest = this.totalPayable - principal;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-IN').format(value);
  }

  resetCalculator(): void {
    this.loanAmount = 2500000;
    this.tenure = 7;
    this.interestRate = 7.75;
    this.interestCalculationType = this.interestSettingService.currentType;

    this.calculateEMI();
  }
}
