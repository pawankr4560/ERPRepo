import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { Loan, LoanEMISchedule, LoanService } from '../services/loan-service';

@Component({
  selector: 'app-loan-details',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatSnackBarModule],
  templateUrl: './loan-details.html',
  styleUrls: ['./loan-details.css'],
})
export class LoanDetailsComponent implements OnInit {
  loan?: Loan;
  isLoading = true;
  loadError = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private loanService: LoanService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(id) || id <= 0) {
      this.isLoading = false;
      this.loadError = 'Invalid loan record.';
      return;
    }

    this.loanService.getLoanById(id).subscribe({
      next: (loan) => {
        this.loan = loan;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.loadError = 'Unable to load this loan. It may have been removed.';
      },
    });
  }

  get schedules(): LoanEMISchedule[] {
    return [...(this.loan?.emiSchedules ?? [])].sort((a, b) => a.installmentNo - b.installmentNo);
  }

  get paidCount(): number {
    return this.schedules.filter((item) => item.isPaid).length;
  }

  get paidProgress(): number {
    return this.schedules.length ? Math.round((this.paidCount / this.schedules.length) * 100) : 0;
  }

  get totalEmi(): number {
    return this.schedules.reduce((total, item) => total + item.emiAmount, 0);
  }

  get totalInterest(): number {
    return this.schedules.reduce((total, item) => total + item.interestAmount, 0);
  }

  getStatusClass(status: string | undefined): string {
    return `status-${(status ?? 'Pending').toLowerCase()}`;
  }

  backToLoans(): void {
    this.router.navigate(['/home/inventory/transactions']);
  }

  print(): void {
    this.openPrintableDocument('Loan Schedule');
  }

  exportPdf(): void {
    this.openPrintableDocument('Loan Schedule - Save as PDF');
  }

  exportExcel(): void {
    if (!this.loan) return;

    const rows = this.schedules.map((item) => `
      <tr><td>${item.installmentNo}</td><td>${this.escapeHtml(this.formatDate(item.dueDate))}</td>
      <td>${item.emiAmount.toFixed(2)}</td><td>${item.principalAmount.toFixed(2)}</td>
      <td>${item.interestAmount.toFixed(2)}</td><td>${item.outstandingBalance.toFixed(2)}</td>
      <td>${item.isPaid ? 'Paid' : 'Pending'}</td></tr>`).join('');

    const workbook = `<html xmlns:x="urn:schemas-microsoft-com:office:excel"><head><meta charset="UTF-8"></head><body>
      <table><tr><th>Loan Number</th><td>${this.escapeHtml(this.loan.loanNumber)}</td></tr>
      <tr><th>Customer</th><td>${this.escapeHtml(this.loan.userName ?? '-')}</td></tr>
      <tr><th>Loan Amount</th><td>${this.loan.loanAmount}</td></tr>
      <tr><th>Interest Rate</th><td>${this.loan.rate ?? 0}%</td></tr></table><br>
      <table border="1"><thead><tr><th>Installment No.</th><th>Due Date</th><th>EMI</th>
      <th>Principal</th><th>Interest</th><th>Balance</th><th>Status</th></tr></thead>
      <tbody>${rows}</tbody></table></body></html>`;

    this.downloadBlob(
      workbook,
      'application/vnd.ms-excel;charset=utf-8',
      `${this.safeFileName(this.loan.loanNumber)}-schedule.xls`
    );
  }

  private openPrintableDocument(title: string): void {
    if (!this.loan) return;
    const target = window.open('', '_blank');
    if (!target) {
      this.snackBar.open('Please allow popups to print or save the PDF.', 'Close', { duration: 4000 });
      return;
    }
    target.document.open();
    target.document.write(this.buildPrintableHtml(title));
    target.document.close();
    target.focus();
    target.print();
  }

  private buildPrintableHtml(title: string): string {
    const loan = this.loan!;
    const rows = this.schedules.map((item) => `
      <tr><td>${item.installmentNo}</td><td>${this.escapeHtml(this.formatDate(item.dueDate))}</td>
      <td>INR ${item.emiAmount.toFixed(2)}</td><td>INR ${item.principalAmount.toFixed(2)}</td>
      <td>INR ${item.interestAmount.toFixed(2)}</td><td>INR ${item.outstandingBalance.toFixed(2)}</td>
      <td>${item.isPaid ? 'Paid' : 'Pending'}</td></tr>`).join('');

    return `<!doctype html><html><head><meta charset="UTF-8"><title>${this.escapeHtml(title)}</title>
      <style>
        body{font-family:Arial,sans-serif;color:#172033;margin:24px}header{display:flex;justify-content:space-between;border-bottom:2px solid #1f3a6f;padding-bottom:16px}
        h1{margin:0 0 6px;color:#1f3a6f}p{margin:4px 0}.summary{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin:20px 0}
        .summary div{border:1px solid #d8dee9;padding:10px}.summary span{display:block;color:#667085;font-size:12px;margin-bottom:4px}
        table{width:100%;border-collapse:collapse}th,td{border:1px solid #d8dee9;padding:8px;text-align:left;font-size:12px}th{background:#eef2f8;color:#273f6a}tfoot td{font-weight:700}
        @page{size:landscape;margin:12mm}
      </style></head><body><header><div><h1>GKFIN PVT LTD</h1>
      <p>Loan schedule: <strong>${this.escapeHtml(loan.loanNumber)}</strong></p></div>
      <div><p><strong>${this.escapeHtml(loan.userName ?? '-')}</strong></p><p>${this.escapeHtml(loan.status ?? 'Pending')}</p></div></header>
      <section class="summary"><div><span>Loan Amount</span><strong>INR ${loan.loanAmount.toFixed(2)}</strong></div>
      <div><span>Interest Rate</span><strong>${Number(loan.rate ?? 0).toFixed(2)}%</strong></div>
      <div><span>Tenure</span><strong>${loan.tenure ?? 0} months</strong></div>
      <div><span>Total Interest</span><strong>INR ${this.totalInterest.toFixed(2)}</strong></div></section>
      <table><thead><tr><th>No.</th><th>Due Date</th><th>EMI</th><th>Principal</th><th>Interest</th><th>Balance</th><th>Status</th></tr></thead>
      <tbody>${rows || '<tr><td colspan="7">No EMI schedule found.</td></tr>'}</tbody>
      <tfoot><tr><td colspan="2">Total</td><td>INR ${this.totalEmi.toFixed(2)}</td><td></td><td>INR ${this.totalInterest.toFixed(2)}</td><td colspan="2"></td></tr></tfoot>
      </table></body></html>`;
  }

  private formatDate(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? '-' : date.toLocaleDateString('en-GB', {
      day: '2-digit', month: 'short', year: 'numeric',
    });
  }

  private escapeHtml(value: string): string {
    return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
  }

  private safeFileName(value: string): string {
    return value.replace(/[^a-z0-9_-]+/gi, '-');
  }

  private downloadBlob(content: string, type: string, fileName: string): void {
    const url = URL.createObjectURL(new Blob([content], { type }));
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
