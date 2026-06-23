import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import {
  LoanService,
  Loan,
  LoanCustomer,
  LoanCustomerDetail,
  LoanEMISchedule,
  LoanPayment,
} from '../services/loan-service';
import { ConfirmDialogComponent } from '../../users/confirm-dialog-component/confirm-dialog-component';
import {
  InterestCalculationType,
  InterestSettingService,
} from '../../setting/interest-setting.service';
import { finalize } from 'rxjs';

interface EmiPreviewRow {
  installmentNo: number;
  dueDate: Date;
  emiAmount: number;
  principalAmount: number;
  interestAmount: number;
  outstandingBalance: number;
}

@Component({
  selector: 'app-loan-component',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatSelectModule,
    MatStepperModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTableModule,
    MatDialogModule,
  ],
  templateUrl: 'loan-component.html',
  styleUrls: ['loan-component.css'],
})
export class LoanComponent implements OnInit {
  loans: Loan[] = [];
  loanSearch = '';
  pageIndex = 0;
  pageSize = 10;

  displayedColumns = ['loanNumber', 'customerName', 'tenureMonths', 'emi', 'dueDate', 'actions'];
  customers: LoanCustomer[] = [];
  isSaving = false;
  isLoading = false;
  isLoadingLoanData = false;
  isLoadingInterestSetting = false;
  isDeleting = false;
  approvalActionLoanId: number | null = null;
  systemInterestType: InterestCalculationType = 'Reducing';
  customerDetail: LoanCustomerDetail = this.createEmptyCustomerDetail();

  // simple form model for create/edit
  editing: boolean = false;
  current: Loan = {
    id: 0,
    userId: '',
    userName: '',
    loanNumber: '',
    loanAmount: 0,
    rate: 0,
    interestCalculationType: 'Reducing',
    tenure: 0,
    emi: 0,
    startDate: this.formatDateInput(new Date()),
    endDate: this.formatDateInput(new Date()),
    status: 'Pending',
    createdDateTime: new Date().toISOString(),
    active: true,
  };

  get tenureYears(): number {
    return this.current?.tenure ? this.current.tenure / 12 : 0;
  }

  set tenureYears(val: number) {
    if (this.current) {
      const years = Number(val);
      this.current.tenure = Number.isFinite(years) ? Math.round(years * 12) : 0;
      this.updateCurrentEmi();
    }
  }

  get hasGuarantor(): boolean {
    return !!this.customerDetail.guarantorName?.trim();
  }

  get isBusy(): boolean {
    return this.isLoading || this.isLoadingLoanData || this.isLoadingInterestSetting ||
      this.isSaving || this.isDeleting || this.approvalActionLoanId !== null;
  }

  get isCustomerAadhaarDuplicate(): boolean {
    const aadhaar = this.customerDetail.customerAadhaarNo?.trim();
    if (aadhaar.length !== 12) {
      return false;
    }

    return this.loans.some(
      (loan) =>
        loan.active !== false &&
        !loan.isDeleted &&
        loan.id !== this.current.id &&
        loan.customerDetail?.customerAadhaarNo === aadhaar
    );
  }

  get emiPreview(): EmiPreviewRow[] {
    const amount = Number(this.current.loanAmount);
    const annualRate = Number(this.current.rate);
    const months = Number(this.current.tenure);
    const startDate = this.current.startDate
      ? new Date(`${this.current.startDate}T00:00:00`)
      : null;

    if (
      amount <= 0 ||
      annualRate < 0 ||
      months <= 0 ||
      !startDate ||
      Number.isNaN(startDate.getTime())
    ) {
      return [];
    }

    return this.buildEmiPreview(
      amount,
      annualRate,
      months,
      startDate,
      this.current.interestCalculationType ?? this.systemInterestType
    );
  }

  get previewTotalInterest(): number {
    return this.emiPreview.reduce((total, row) => total + row.interestAmount, 0);
  }

  get previewTotalPayable(): number {
    return this.emiPreview.reduce((total, row) => total + row.emiAmount, 0);
  }

  private originalLoanNumber?: string;

  constructor(
    private loanService: LoanService,
    private dialog: MatDialog,
    private interestSettingService: InterestSettingService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loanService.loans$.subscribe((l) => (this.loans = l ?? []));
    this.isLoadingInterestSetting = true;
    this.interestSettingService.load()
      .pipe(finalize(() => (this.isLoadingInterestSetting = false)))
      .subscribe((type) => {
      this.systemInterestType = type;
      if (!this.current.id) {
        this.current.interestCalculationType = type;
        this.updateCurrentEmi();
      }
    });
    this.load();
  }

  load() {
    this.isLoading = true;
    this.loanService.loadLoans()
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        error: (error) => this.showApiError('Unable to load loans.', error),
      });
  }

  get filteredLoans(): Loan[] {
    const query = this.loanSearch?.trim().toLowerCase();
    if (!query) {
      return this.loans;
    }

    return this.loans.filter((loan) => {
      const loanNumber = loan.loanNumber?.toString().toLowerCase() ?? '';
      const userName = loan.userName?.toString().toLowerCase() ?? '';
      const amount = loan.loanAmount?.toString().toLowerCase() ?? '';
      const status = loan.status?.toString().toLowerCase() ?? '';
      const startDate = loan.startDate?.toString().toLowerCase() ?? '';
      const endDate = loan.endDate?.toString().toLowerCase() ?? '';
      return (
        loanNumber.includes(query) ||
        userName.includes(query) ||
        amount.includes(query) ||
        status.includes(query) ||
        startDate.includes(query) ||
        endDate.includes(query)
      );
    });
  }

  get pageCount(): number {
    return Math.ceil(this.filteredLoans.length / this.pageSize);
  }

  private get normalizedPageIndex(): number {
    return this.pageCount === 0 ? 0 : Math.min(this.pageIndex, this.pageCount - 1);
  }

  get pagedLoans(): Loan[] {
    const start = this.normalizedPageIndex * this.pageSize;
    return this.filteredLoans.slice(start, start + this.pageSize);
  }

  get currentPageNumber(): number {
    return this.normalizedPageIndex + 1;
  }

  get startRecord(): number {
    return this.filteredLoans.length === 0 ? 0 : this.normalizedPageIndex * this.pageSize + 1;
  }

  get endRecord(): number {
    return Math.min((this.normalizedPageIndex + 1) * this.pageSize, this.filteredLoans.length);
  }

  onSearchChange(value: string) {
    this.loanSearch = value?.trim() ?? '';
    this.pageIndex = 0;
  }

  onCustomerChange(userId: string) {
    this.current.userId = userId;
    const selected = this.customers.find((customer) => customer.id === userId);
    this.current.userName = selected?.customerName ?? '';
  }

  openLoanDetails(loan: Loan): void {
    if (loan.id == null) {
      return;
    }

    this.router.navigate(['/home/inventory/transactions', loan.id]);
  }

  get selectedLoanCount(): number {
    return this.loans.filter((loan) => loan.isSelected).length;
  }

  get selectedLoan(): Loan | undefined {
    return this.loans.find((loan) => loan.isSelected);
  }

  get selectedSchedules(): LoanEMISchedule[] {
    return [...(this.selectedLoan?.emiSchedules ?? [])].sort(
      (a, b) => (a.installmentNo ?? 0) - (b.installmentNo ?? 0)
    );
  }

  get selectedPayments(): LoanPayment[] {
    return [...(this.selectedLoan?.payments ?? [])].sort(
      (a, b) => new Date(b.paymentDate).getTime() - new Date(a.paymentDate).getTime()
    );
  }

  get paidScheduleCount(): number {
    return this.selectedSchedules.filter((schedule) => schedule.isPaid).length;
  }

  get paidProgress(): number {
    return this.selectedSchedules.length
      ? Math.round((this.paidScheduleCount / this.selectedSchedules.length) * 100)
      : 0;
  }

  getLoanScheduleCount(loan: Loan): number {
    return loan.emiSchedules?.length ?? 0;
  }

  getLoanPaidCount(loan: Loan): number {
    return loan.emiSchedules?.filter((schedule) => schedule.isPaid).length ?? 0;
  }

  getLoanPaidProgress(loan: Loan): number {
    const total = this.getLoanScheduleCount(loan);
    return total ? Math.round((this.getLoanPaidCount(loan) / total) * 100) : 0;
  }

  onLoanSelected(loan: Loan) {
    const wasSelected = !!loan.isSelected;
    this.loans.forEach((item) => {
      item.isSelected = false;
    });
    if (!wasSelected) {
      loan.isSelected = true;
    }
  }

  getStatusClass(status: string | null | undefined): string {
    return `status-${(status ?? 'Pending').toLowerCase()}`;
  }

  isPending(loan: Loan): boolean {
    return (loan.status ?? 'Pending').toLowerCase() === 'pending';
  }

  isApproved(loan: Loan): boolean {
    return (loan.status ?? '').toLowerCase() === 'active';
  }

  approve(loan: Loan): void {
    if (!loan.id || !this.isPending(loan) || this.approvalActionLoanId !== null) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Approve Loan',
        message: `Approve ${loan.loanNumber}? This will activate the loan and generate its EMI schedule.`,
        confirmText: 'Approve',
        icon: 'check_circle',
        confirmColor: 'primary',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed || !loan.id) {
        return;
      }

      this.approvalActionLoanId = loan.id;
      this.loanService.approveLoan(loan.id)
        .pipe(finalize(() => (this.approvalActionLoanId = null)))
        .subscribe({
          next: (response) => {
            this.snackBar.open(response?.message || 'Loan approved successfully.', 'Close', {
              duration: 3000,
            });
            this.load();
          },
          error: (error) => this.showApiError('Unable to approve loan.', error),
        });
    });
  }

  reject(loan: Loan): void {
    if (!loan.id || !this.isPending(loan) || this.approvalActionLoanId !== null) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Reject Loan',
        message: `Reject ${loan.loanNumber}? The loan will remain inactive and no EMI schedule will be created.`,
        confirmText: 'Reject',
        icon: 'cancel',
        confirmColor: 'warn',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed || !loan.id) {
        return;
      }

      this.approvalActionLoanId = loan.id;
      this.loanService.rejectLoan(loan.id)
        .pipe(finalize(() => (this.approvalActionLoanId = null)))
        .subscribe({
          next: (response) => {
            this.snackBar.open(response?.message || 'Loan rejected successfully.', 'Close', {
              duration: 3000,
            });
            this.load();
          },
          error: (error) => this.showApiError('Unable to reject loan.', error),
        });
    });
  }

  private buildPrintPage(loan: Loan): string {
    const schedule = this.generateInstallmentSchedule(loan);
    const totalEMI = schedule.reduce((sum, item) => sum + item.emi, 0);
    const loanAmount = loan.loanAmount ?? 0;
    const rate = loan.rate ?? 0;
    const tenureYears = loan.tenure ? loan.tenure / 12 : 0;

    return `
      <div class="print-page">
        <div class="print-card">
          <div class="print-header">
            <div>
              <h1>GKFIN PVT LTD</h1>
              <div class="print-subtitle">Generated for loan <strong>${loan.loanNumber}</strong></div>
            </div>
            <div class="print-meta">
              <div><strong>Customer</strong><span>${loan.userName ?? '-'}</span></div>
              <div><strong>Loan Amount</strong><span>₹${loanAmount.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span></div>
              <div><strong>Rate</strong><span>${rate.toFixed(2)}%</span></div>
              <div><strong>Tenure</strong><span>${tenureYears} years</span></div>
            </div>
          </div>

          <table class="print-table">
            <thead>
              <tr>
                <th>Month</th>
                <th>EMI</th>
                <th>Due Date</th>
              </tr>
            </thead>
            <tbody>
              ${schedule
                .map(
                  (item) => `
                    <tr>
                      <td>${item.monthLabel}</td>
                      <td>₹${item.emi.toFixed(2)}</td>
                      <td>${item.dueDate}</td>
                    </tr>`
                )
                .join('')}
            </tbody>
            <tfoot>
              <tr>
                <td><strong>Total</strong></td>
                <td><strong>₹${totalEMI.toFixed(2)}</strong></td>
                <td></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>`;
  }

  private getPrintStyles(): string {
    return `
      <style>
        body { margin: 0; padding: 0; font-family: Arial, sans-serif; color: #1f2937; background: #f6f8fb; }
        .print-page { padding: 24px; }
        .print-card { background: #ffffff; border-radius: 12px; box-shadow: 0 16px 48px rgba(16, 24, 40, 0.08); padding: 28px; }
        .print-header { display: flex; justify-content: space-between; flex-wrap: wrap; gap: 16px; border-bottom: 1px solid #e5e7eb; padding-bottom: 18px; margin-bottom: 24px; }
        .print-header h1 { margin: 0; font-size: 28px; letter-spacing: -0.02em; }
        .print-subtitle { color: #4b5563; margin-top: 6px; }
        .print-meta { display: flex; flex-wrap: wrap; gap: 16px; justify-content: flex-end; }
        .print-meta div { min-width: 160px; }
        .print-meta strong { display: block; color: #111827; margin-bottom: 4px; font-size: 13px; }
        .print-meta span { color: #374151; font-size: 15px; }
        .print-table { width: 100%; border-collapse: collapse; margin-top: 8px; }
        .print-table th,
        .print-table td { border: 1px solid #e5e7eb; padding: 10px 8px; text-align: left; font-size: 12px; }
        .print-table th { background: #eff6ff; color: #1e3a8a; font-weight: 700; }
        .print-table tbody tr:nth-child(even) { background: #f8fafc; }
        .print-table tfoot td { border-top: 2px solid #d1d5db; }
        .print-table tfoot td strong { font-size: 15px; }
        .print-status { display: inline-block; min-width: 62px; padding: 4px 8px; border-radius: 4px; text-align: center; font-weight: 700; }
        .print-status-success { color: #1b5e20; background: #e8f5e9; border: 1px solid #81c784; }
        .print-status-pending { color: #b42318; background: #ffebee; border: 1px solid #ef9a9a; }
        @media print {
          @page { size: landscape; margin: 12mm; }
          body { background: #fff; }
          .print-page { padding: 0; }
          .print-card { box-shadow: none; border-radius: 0; }
        }
      </style>
    `;
  }

  private buildApiPrintPage(
    loan: Loan,
    schedule: LoanEMISchedule[]
  ): string {
    const totalEMI = schedule.reduce((sum, item) => sum + item.emiAmount, 0);
    const totalPrincipal = schedule.reduce(
      (sum, item) => sum + item.principalAmount,
      0
    );
    const totalInterest = schedule.reduce(
      (sum, item) => sum + item.interestAmount,
      0
    );

    return `
      <div class="print-page">
        <div class="print-card">
          <div class="print-header">
            <div>
              <h1>GKFIN PVT LTD</h1>
              <div class="print-subtitle">Loan <strong>${loan.loanNumber}</strong></div>
            </div>
            <div class="print-meta">
              <div><strong>Customer</strong><span>${loan.userName ?? '-'}</span></div>
              <div><strong>Loan Amount</strong><span>INR ${loan.loanAmount.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span></div>
              <div><strong>Rate</strong><span>${Number(loan.rate ?? 0).toFixed(2)}%</span></div>
              <div><strong>Interest Type</strong><span>${loan.interestCalculationType ?? '-'}</span></div>
            </div>
          </div>
          <table class="print-table">
            <thead>
              <tr>
                <th>No.</th>
                <th>Due Date</th>
                <th>EMI</th>
                <th>Principal</th>
                <th>Interest</th>
                <th>Balance</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              ${schedule
                .map(
                  (item) => `
                    <tr>
                      <td>${item.installmentNo}</td>
                      <td>${this.formatPrintDate(item.dueDate)}</td>
                      <td>INR ${item.emiAmount.toFixed(2)}</td>
                      <td>INR ${item.principalAmount.toFixed(2)}</td>
                      <td>INR ${item.interestAmount.toFixed(2)}</td>
                      <td>INR ${item.outstandingBalance.toFixed(2)}</td>
                      <td>
                        <span class="print-status ${item.isPaid ? 'print-status-success' : 'print-status-pending'}">
                          ${item.isPaid ? 'Success' : 'Pending'}
                        </span>
                        ${item.isPaid && item.paidDate ? `<div>Paid on ${this.formatPrintDate(item.paidDate)}</div>` : ''}
                      </td>
                    </tr>`
                )
                .join('')}
            </tbody>
            <tfoot>
              <tr>
                <td><strong>Total</strong></td>
                <td></td>
                <td><strong>INR ${totalEMI.toFixed(2)}</strong></td>
                <td><strong>INR ${totalPrincipal.toFixed(2)}</strong></td>
                <td><strong>INR ${totalInterest.toFixed(2)}</strong></td>
                <td></td>
                <td></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>`;
  }

  private renderPrintWindow(
    targetWindow: Window,
    loan: Loan,
    schedule: LoanEMISchedule[],
    title: string
  ): void {
    if (!schedule.length) {
      targetWindow.close();
      this.snackBar.open('No EMI schedule was returned for this loan.', 'Close', {
        duration: 4000,
      });
      return;
    }

    targetWindow.document.open();
    targetWindow.document.write(`
      <html>
        <head>
          <title>${title}</title>
          ${this.getPrintStyles()}
        </head>
        <body>${this.buildApiPrintPage(loan, schedule)}</body>
      </html>
    `);
    targetWindow.document.close();
    targetWindow.focus();
    targetWindow.print();
  }

  private writeLoadingPage(targetWindow: Window, message: string): void {
    targetWindow.document.write(
      `<html><body style="font-family:Arial;padding:32px">${message}</body></html>`
    );
    targetWindow.document.close();
  }

  private formatPrintDate(value: string): string {
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? '-'
      : date.toLocaleDateString('en-GB', {
          day: '2-digit',
          month: 'short',
          year: 'numeric',
        });
  }

  printSelected() {
    const loan = this.selectedLoan;
    if (!loan) {
      return;
    }

    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      this.snackBar.open('Please allow popups to print the loan schedule.', 'Close', {
        duration: 4000,
      });
      return;
    }

    this.writeLoadingPage(printWindow, 'Loading loan schedule...');
    this.loanService.getScheduleByLoanNumber(loan.loanNumber).subscribe({
      next: (schedule) =>
        this.renderPrintWindow(
          printWindow,
          loan,
          schedule,
          'GKFIN PVT LTD'
        ),
      error: (error) => {
        printWindow.close();
        this.showApiError('Unable to load the loan schedule for printing.', error);
      },
    });
  }

  exportSelectedAsPdf() {
    const loan = this.selectedLoan;
    if (!loan) {
      return;
    }

    const newWindow = window.open('', '_blank');
    if (!newWindow) {
      this.snackBar.open('Please allow popups to export the schedule.', 'Close', {
        duration: 4000,
      });
      return;
    }

    this.writeLoadingPage(newWindow, 'Preparing PDF schedule...');
    this.loanService.getScheduleByLoanNumber(loan.loanNumber).subscribe({
      next: (schedule) =>
        this.renderPrintWindow(newWindow, loan, schedule, 'Loan Export PDF'),
      error: (error) => {
        newWindow.close();
        this.showApiError('Unable to load the loan schedule for export.', error);
      },
    });
  }

  exportSelectedAsExcel() {
    const selected = this.selectedLoan ? [this.selectedLoan] : [];
    if (!selected.length) {
      return;
    }

    // Export the full installment schedule: one CSV row per month
    const header = [
      'Loan Number',
      'Customer',
      'Loan Amount',
      'Rate',
      'Tenure (Years)',
      'Month',
      'EMI',
      'Due Date',
    ];

    const rows: string[][] = selected.flatMap((loan) => {
      const schedule = this.generateInstallmentSchedule(loan);
      const loanAmount = loan.loanAmount?.toString() ?? '';
      const rate = loan.rate?.toString() ?? '';
      const tenureYears = loan.tenure ? (loan.tenure / 12).toString() : '0';
      const customer = loan.userName ?? '';
      const loanNumber = loan.loanNumber ?? '';

      return schedule.map((item) => [
        loanNumber,
        customer,
        loanAmount,
        rate,
        tenureYears,
        item.monthLabel,
        item.emi.toFixed(2),
        item.dueDate,
      ]);
    });

    const csvContent = [header, ...rows]
      .map((row) => row.map((value) => `"${String(value).replace(/"/g, '""')}"`).join(','))
      .join('\r\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = 'loan-export.csv';
    anchor.click();
    URL.revokeObjectURL(url);
  }

  clearSearch() {
    this.loanSearch = '';
    this.pageIndex = 0;
  }

  private loadLoanData() {
    this.isLoadingLoanData = true;
    this.loanService.getLoanData()
      .pipe(finalize(() => (this.isLoadingLoanData = false)))
      .subscribe({
      next: (data) => {
        this.customers = data.customerList ?? [];
        if (!this.current.id) {
          this.current.loanNumber = data.loanNumber ?? '';
        }

        if (!this.current.userId && this.current.userName) {
          const matchingCustomer = this.customers.find(
            (customer) => customer.customerName === this.current.userName || customer.id === this.current.userId
          );
          if (matchingCustomer) {
            this.current.userId = matchingCustomer.id;
            this.current.userName = matchingCustomer.customerName;
          }
        }
      },
      error: (error) => this.showApiError('Unable to load loan form data.', error),
    });
  }

  firstPage() {
    this.pageIndex = 0;
  }

  previousPage() {
    if (this.currentPageNumber > 1) {
      this.pageIndex = this.normalizedPageIndex - 1;
    }
  }

  nextPage() {
    if (this.currentPageNumber < this.pageCount) {
      this.pageIndex = this.normalizedPageIndex + 1;
    }
  }

  lastPage() {
    if (this.pageCount > 0) {
      this.pageIndex = this.pageCount - 1;
    }
  }

  startCreate() {
    this.editing = true;
    this.current = {
      id: 0,
      userId: '',
      userName: '',
      loanNumber: '',
      loanAmount: 0,
      rate: 0,
      interestCalculationType: this.systemInterestType,
      tenure: 0,
      emi: 0,
      createdDateTime: new Date().toISOString(),
      active: true,
      startDate: this.formatDateInput(new Date()),
      endDate: this.formatDateInput(new Date()),
      status: 'Pending',
    };
    this.customerDetail = this.createEmptyCustomerDetail();

    this.loadLoanData();
  }

  startEdit(loan: Loan) {
    if (this.isApproved(loan)) {
      return;
    }

    this.editing = true;
    this.current = { ...loan };
    this.current.startDate = this.toDateInputValue(loan.startDate);
    this.current.endDate = this.toDateInputValue(loan.endDate);
    this.originalLoanNumber = loan.loanNumber;
    this.customerDetail = loan.customerDetail
      ? { ...loan.customerDetail }
      : this.createEmptyCustomerDetail();
    this.loadLoanData();
  }

  cancel() {
    this.editing = false;
    this.current = {
      id: 0,
      userId: '',
      userName: '',
      loanNumber: '',
      loanAmount: 0,
      rate: 0,
      interestCalculationType: this.systemInterestType,
      tenure: 0,
      emi: 0,
      startDate: this.formatDateInput(new Date()),
      endDate: this.formatDateInput(new Date()),
      status: 'Pending',
      createdDateTime: new Date().toISOString(),
      active: true,
    };
    this.customerDetail = this.createEmptyCustomerDetail();
    this.originalLoanNumber = undefined;
  }

  save(form: NgForm) {
    form.control.markAllAsTouched();

    if (form.invalid || !this.isLoanInputValid() || this.isSaving) {
      return;
    }

    if (!this.current.createdDateTime) {
      this.current.createdDateTime = new Date().toISOString();
    }

    if (!this.current.startDate) {
      this.current.startDate = this.formatDateInput(new Date());
    }

    if (!this.current.emi || this.current.emi === 0) {
      this.current.emi = this.calculateEMI(
        this.current.loanAmount,
        this.current.rate ?? 0,
        this.current.tenure ?? 0,
        this.current.interestCalculationType
      );
    }

    if (this.current.startDate && this.current.tenure) {
      this.current.endDate = this.formatDateInput(
        this.addMonths(new Date(this.current.startDate), this.current.tenure)
      );
    }

    this.isSaving = true;

    // Backend distinguishes create vs update based on id.
    // In this UI, create starts with id=0, so treat 0 as CREATE.
    const canUpdate = this.current.id != null && Number.isFinite(this.current.id) && this.current.id !== 0;

    if (canUpdate) {
      // Ensure loanNumber is not modified when updating
      if (this.originalLoanNumber) {
        this.current.loanNumber = this.originalLoanNumber;
      }
      this.loanService.updateLoan(this.current).subscribe({
        next: () => {
          this.isSaving = false;
          this.cancel();
          this.load();
        },
        error: (e) => {
          this.isSaving = false;
          console.error(e);
        },
      });
    } else {
      this.loanService.createLoan(this.current).subscribe({
        next: () => {
          this.isSaving = false;
          this.cancel();
          this.load();
        },
        error: (e) => {
          this.isSaving = false;
          console.error(e);
        },
      });
    }
  }

  private isLoanInputValid(): boolean {
    const amount = Number(this.current.loanAmount);
    const rate = Number(this.current.rate);
    const tenure = Number(this.current.tenure);

    return (
      !!this.current.userId?.trim() &&
      Number.isFinite(amount) &&
      amount > 0 &&
      Number.isFinite(rate) &&
      rate > 0 &&
      rate <= 100 &&
      Number.isFinite(tenure) &&
      tenure >= 6 &&
      tenure <= 600 &&
      !!this.current.status?.trim()
    );
  }

  saveWizard(): void {
    if (this.isSaving || !this.isWizardValid()) {
      this.snackBar.open('Please complete all required fields before saving.', 'Close', {
        duration: 4000,
        panelClass: ['error-snackbar'],
      });
      return;
    }

    this.current.customerDetail = { ...this.customerDetail };
    this.current.emi = this.emiPreview[0]?.emiAmount ?? 0;
    if (this.current.startDate && this.current.tenure) {
      this.current.endDate = this.formatDateInput(
        this.addMonths(
          new Date(`${this.current.startDate}T00:00:00`),
          this.current.tenure
        )
      );
    }

    this.isSaving = true;
    this.loanService.createLoan(this.current).subscribe({
      next: () => {
        this.isSaving = false;
        this.snackBar.open('Loan created successfully.', 'Close', { duration: 3000 });
        this.cancel();
        this.load();
      },
      error: (error) => {
        this.isSaving = false;
        this.showApiError('Loan creation failed.', error);
      },
    });
  }

  isCustomerStepValid(): boolean {
    return !!this.current.userId?.trim() && !!this.current.loanNumber?.trim();
  }

  isVerificationStepValid(): boolean {
    return (
      /^\d{12}$/.test(this.customerDetail.customerAadhaarNo ?? '') &&
      /^\d{10}$/.test(this.customerDetail.customerMobileNo ?? '') &&
      !!this.customerDetail.customerAddress?.trim() &&
      !this.isCustomerAadhaarDuplicate
    );
  }

  isGuarantorStepValid(): boolean {
    if (!this.hasGuarantor) {
      return true;
    }

    return (
      /^\d{12}$/.test(this.customerDetail.guarantorAadhaarNo ?? '') &&
      /^\d{10}$/.test(this.customerDetail.guarantorMobileNo ?? '')
    );
  }

  isLoanStepValid(): boolean {
    return (
      Number(this.current.loanAmount) > 0 &&
      Number(this.current.rate) > 0 &&
      Number(this.current.rate) <= 100 &&
      Number(this.current.tenure) >= 6 &&
      Number(this.current.tenure) <= 600 &&
      !!this.current.startDate &&
      !!this.current.interestCalculationType
    );
  }

  isWizardValid(): boolean {
    return (
      this.isCustomerStepValid() &&
      this.isVerificationStepValid() &&
      this.isGuarantorStepValid() &&
      this.isLoanStepValid() &&
      this.emiPreview.length > 0
    );
  }

  remove(loan: Loan) {
    if (!loan || this.isApproved(loan) || loan.id == null || !Number.isFinite(loan.id)) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '380px',
      data: {
        title: 'Delete Loan',
        message: `Are you sure you want to delete loan "${loan.loanNumber}"?`,
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.isDeleting = true;
        this.loanService.deleteLoan(loan.id as number)
          .pipe(finalize(() => (this.isDeleting = false)))
          .subscribe({
          next: () => {
            this.load();
          },
          error: (error) => this.showApiError('Unable to delete the loan.', error),
        });
      }
    });
  }

  updateCurrentEmi() {
    if (!this.current) {
      return;
    }

    this.current.emi = this.calculateEMI(
      this.current.loanAmount,
      this.current.rate ?? 0,
      this.current.tenure ?? 0,
      this.current.interestCalculationType
    );
    if (this.current.startDate && this.current.tenure) {
      this.current.endDate = this.formatDateInput(
        this.addMonths(new Date(this.current.startDate), this.current.tenure)
      );
    }
  }

  loanEMI(loan: Loan): number {
    return (
      loan.emi ??
      this.calculateEMI(
        loan.loanAmount,
        loan.rate ?? 0,
        loan.tenure ?? 0,
        loan.interestCalculationType
      )
    );
  }

  getLoanStartDateText(loan: Loan): string {
    return this.formatDisplayDate(loan.startDate);
  }

  getLoanEndDateText(loan: Loan): string {
    const endDate = this.parseBusinessDate(loan.endDate);
    if (endDate) {
      return this.formatDisplayDate(endDate.toISOString());
    }

    const startDate = this.parseBusinessDate(loan.startDate);
    if (startDate && loan.tenure) {
      return this.formatDisplayDate(this.addMonths(startDate, loan.tenure).toISOString());
    }

    return 'Not set';
  }

  getLoanMonthLabel(loan: Loan): string {
    const createdDate = loan.createdDateTime ? new Date(loan.createdDateTime) : new Date();
    if (Number.isNaN(createdDate.getTime())) {
      return '';
    }
    return this.monthLabel(createdDate);
  }

  generateInstallmentSchedule(loan: Loan) {
    const emi = this.loanEMI(loan);
    const createdDate = loan.createdDateTime ? new Date(loan.createdDateTime) : new Date();
    const monthCount = loan.tenure ?? 0;

    const schedule = [] as Array<{ monthLabel: string; emi: number; dueDate: string }>;

    for (let i = 0; i < monthCount; i++) {
      const dueDateObj = this.addMonths(createdDate, i + 1);
      const monthLabel = this.monthLabel(dueDateObj);
      const dueDate = dueDateObj.toLocaleDateString();
      schedule.push({ monthLabel, emi, dueDate });
    }

    return schedule;
  }

  getLoanDueDate(loan: Loan): string {
    const createdDate = loan.createdDateTime ? new Date(loan.createdDateTime) : new Date();
    if (Number.isNaN(createdDate.getTime())) {
      return '';
    }

    return this.addMonths(createdDate, 1).toLocaleDateString();
  }

  private addMonths(date: Date, months: number): Date {
    const result = new Date(date);
    result.setMonth(result.getMonth() + months);
    return result;
  }

  private monthLabel(date: Date): string {
    return date.toLocaleString('en-US', { month: 'short' });
  }

  private formatDisplayDate(value: string | undefined): string {
    const date = this.parseBusinessDate(value);
    if (!date) {
      return 'Not set';
    }

    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }

  private parseBusinessDate(value: string | undefined): Date | null {
    if (!value) {
      return null;
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime()) || date.getFullYear() < 1900) {
      return null;
    }

    return date;
  }

  private formatDateInput(date: Date): string {
    if (Number.isNaN(date.getTime())) {
      return '';
    }

    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private toDateInputValue(value: string | undefined): string {
    if (!value) {
      return '';
    }

    return this.formatDateInput(new Date(value));
  }

  calculateEMI(
    amount: number,
    rate: number,
    tenureMonths: number,
    type: InterestCalculationType = this.systemInterestType
  ): number {
    return this.interestSettingService.calculateEmi(amount, rate, tenureMonths, type);
  }

  private createEmptyCustomerDetail(): LoanCustomerDetail {
    return {
      customerAadhaarNo: '',
      customerMobileNo: '',
      customerAddress: '',
      customerCity: '',
      customerState: '',
      customerPinCode: '',
      guarantorName: '',
      guarantorAadhaarNo: '',
      guarantorMobileNo: '',
      guarantorAddress: '',
      guarantorRelationship: '',
    };
  }

  private buildEmiPreview(
    amount: number,
    annualRate: number,
    months: number,
    startDate: Date,
    type: InterestCalculationType
  ): EmiPreviewRow[] {
    const rows: EmiPreviewRow[] = [];
    let balance = amount;

    if (type === 'Flat') {
      const monthlyPrincipal = amount / months;
      const monthlyInterest =
        (amount * annualRate * months) / 12 / 100 / months;

      for (let index = 0; index < months; index++) {
        const principal = index === months - 1 ? balance : monthlyPrincipal;
        balance = Math.max(0, balance - principal);
        rows.push({
          installmentNo: index + 1,
          dueDate: this.addMonths(startDate, index),
          emiAmount: this.roundMoney(principal + monthlyInterest),
          principalAmount: this.roundMoney(principal),
          interestAmount: this.roundMoney(monthlyInterest),
          outstandingBalance: this.roundMoney(balance),
        });
      }
      return rows;
    }

    const monthlyRate = annualRate / 1200;
    const emi = this.calculateEMI(amount, annualRate, months, 'Reducing');
    for (let index = 0; index < months; index++) {
      const interest = balance * monthlyRate;
      const principal =
        index === months - 1 ? balance : Math.min(balance, emi - interest);
      balance = Math.max(0, balance - principal);
      rows.push({
        installmentNo: index + 1,
        dueDate: this.addMonths(startDate, index),
        emiAmount: this.roundMoney(principal + interest),
        principalAmount: this.roundMoney(principal),
        interestAmount: this.roundMoney(interest),
        outstandingBalance: this.roundMoney(balance),
      });
    }
    return rows;
  }

  private roundMoney(value: number): number {
    return Number(value.toFixed(2));
  }

  private showApiError(fallback: string, error: any): void {
    const validationErrors = error?.error?.errors
      ? Object.values(error.error.errors).flat().join(' ')
      : '';
    this.snackBar.open(
      error?.error?.errorMessage ||
        error?.error?.message ||
        validationErrors ||
        fallback,
      'Close',
      { duration: 5000, panelClass: ['error-snackbar'] }
    );
  }
}

@Component({
  selector: 'app-loan-delete-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatDialogModule],
  template: `
    <div class="delete-dialog">
      <div class="dialog-icon">
        <mat-icon>warning</mat-icon>
      </div>
      <h2 mat-dialog-title>Delete Loan</h2>
      <mat-dialog-content>
        Are you sure you want to delete loan <strong>{{ data.loanNumber }}</strong>? This action cannot be undone.
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button (click)="onCancel()">Cancel</button>
        <button mat-raised-button color="warn" (click)="onConfirm()">Delete</button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [
    `
      .delete-dialog {
        text-align: center;
      }
      .dialog-icon {
        font-size: 48px;
        color: #f44336;
        margin-bottom: 12px;
      }
      .dialog-icon mat-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        color: #f44336;
      }
      mat-dialog-content {
        margin: 20px 0;
      }
      strong {
        color: #1f3a6f;
      }
    `,
  ],
})
export class LoanDeleteConfirmDialog {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { loanNumber: string },
    private dialogRef: MatDialogRef<LoanDeleteConfirmDialog>
  ) {}

  onConfirm() {
    this.dialogRef.close(true);
  }

  onCancel() {
    this.dialogRef.close(false);
  }
}

