import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
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
    MatSelectModule,
    MatIconModule
  ],
  templateUrl: './emi.html',
  styleUrls: ['./emi.css'],
})
export class Emi implements OnInit {
  loanAmount: number = 500000;
  tenure: number = 5;
  interestRate: number = 8.5;
  processingFeeRate: number = 1;
  selectedLoanType = 'Home';
  scheduleView: 'Monthly' | 'Yearly' = 'Monthly';
  loanTypes = ['Home', 'Car', 'Education', 'Personal'];

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

  get processingFee(): number {
    return (this.loanAmount * this.processingFeeRate) / 100;
  }

  get principalPercentage(): number {
    return this.totalPayable ? Math.round((this.loanAmount / this.totalPayable) * 100) : 0;
  }

  get interestPercentage(): number {
    return this.totalPayable ? Math.round((this.totalInterest / this.totalPayable) * 100) : 0;
  }

  get amortizationRows(): Array<{
    month: number;
    emi: number;
    principal: number;
    interest: number;
    balance: number;
    progress: number;
  }> {
    const rows = [];
    const months = this.tenure * 12;
    const monthlyRate = this.interestRate / 1200;
    let balance = this.loanAmount;

    for (let index = 1; index <= Math.min(months, 8); index++) {
      const interest = this.interestCalculationType === 'Flat'
        ? this.totalInterest / months
        : balance * monthlyRate;
      const principal = Math.min(balance, this.emi - interest);
      balance = Math.max(0, balance - principal);
      rows.push({
        month: index,
        emi: this.emi,
        principal,
        interest,
        balance,
        progress: Math.min(100, (index / months) * 100),
      });
    }

    return rows;
  }

  get yearlyInterestRows(): Array<{ year: number; interest: number; progress: number }> {
    const rows = [];
    const monthlyRate = this.interestRate / 1200;
    let balance = this.loanAmount;

    for (let year = 1; year <= Math.min(this.tenure, 5); year++) {
      let interestTotal = 0;
      for (let month = 1; month <= 12; month++) {
        const interest = this.interestCalculationType === 'Flat'
          ? this.totalInterest / (this.tenure * 12)
          : balance * monthlyRate;
        const principal = Math.min(balance, this.emi - interest);
        interestTotal += interest;
        balance = Math.max(0, balance - principal);
      }
      rows.push({
        year,
        interest: interestTotal,
        progress: this.totalInterest ? Math.min(100, (interestTotal / this.totalInterest) * 100) : 0,
      });
    }

    return rows;
  }

  setLoanType(type: string): void {
    this.selectedLoanType = type;
  }

  setScheduleView(view: 'Monthly' | 'Yearly'): void {
    this.scheduleView = view;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-IN', { maximumFractionDigits: 0 }).format(value || 0);
  }

  formatCompactCurrency(value: number): string {
    if (value >= 100000) {
      return `${(value / 100000).toFixed(value % 100000 === 0 ? 0 : 2)}L`;
    }
    return this.formatCurrency(value);
  }

  resetCalculator(): void {
    this.loanAmount = 500000;
    this.tenure = 5;
    this.interestRate = 8.5;
    this.processingFeeRate = 1;
    this.interestCalculationType = this.interestSettingService.currentType;

    this.calculateEMI();
  }
}
