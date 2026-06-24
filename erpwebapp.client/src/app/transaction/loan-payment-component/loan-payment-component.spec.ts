import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { LoanPaymentComponent } from './loan-payment-component';
import { Loan, LoanPayment, LoanService } from '../services/loan-service';
import { LoanPaymentService } from '../services/loan-payment-service';

describe('LoanPaymentComponent', () => {
  let component: LoanPaymentComponent;
  let fixture: ComponentFixture<LoanPaymentComponent>;
  let loanService: jasmine.SpyObj<LoanService>;
  let paymentService: jasmine.SpyObj<LoanPaymentService>;
  let dialog: jasmine.SpyObj<MatDialog>;
  let snackBar: jasmine.SpyObj<MatSnackBar>;
  let loans$: BehaviorSubject<Loan[]>;
  let payments$: BehaviorSubject<LoanPayment[]>;

  const testLoan: Loan = {
    id: 1, userId: 1, userName: 'Customer', loanNumber: 'LN-1',
    loanAmount: 10000, rate: 10, tenure: 2, emi: 5250,
    emiSchedules: [{ id: 11, loanId: 1, installmentNo: 1, dueDate: '2026-01-01', emiAmount: 5250, principalAmount: 5000, interestAmount: 250, outstandingBalance: 5000, isPaid: false }],
  };
  const payment: LoanPayment = {
    id: 5, loanId: 1, scheduleId: 11, amountPaid: 5250,
    paymentDate: '2026-01-02T10:00:00', transactionId: 'TX-5',
    paymentMode: 'Cash', paymentStatus: 'Success', remarks: null,
  };

  beforeEach(async () => {
    loans$ = new BehaviorSubject<Loan[]>([testLoan]);
    payments$ = new BehaviorSubject<LoanPayment[]>([payment]);
    loanService = jasmine.createSpyObj<LoanService>('LoanService', ['loadLoans'], { loans$: loans$.asObservable() });
    paymentService = jasmine.createSpyObj<LoanPaymentService>(
      'LoanPaymentService',
      ['loadPayments', 'getPayment', 'getUnpaidInstallments', 'createPayment', 'updatePayment', 'deletePayment'],
      { payments$: payments$.asObservable() }
    );
    dialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    snackBar = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);
    loanService.loadLoans.and.returnValue(of([testLoan]));
    paymentService.loadPayments.and.returnValue(of([payment]));

    await TestBed.configureTestingModule({
      imports: [LoanPaymentComponent],
      providers: [
        { provide: LoanService, useValue: loanService },
        { provide: LoanPaymentService, useValue: paymentService },
        { provide: MatDialog, useValue: dialog },
        { provide: MatSnackBar, useValue: snackBar },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(LoanPaymentComponent);
    component = fixture.componentInstance;
    (component as any).dialog = dialog;
    (component as any).snackBar = snackBar;
    fixture.detectChanges();
  });

  it('loads loans and payments on initialization', () => {
    expect(loanService.loadLoans).toHaveBeenCalled();
    expect(paymentService.loadPayments).toHaveBeenCalled();
    expect(component.payments.length).toBe(1);
  });

  it('keeps the refresh loader active until both APIs complete', () => {
    const loansRequest = new Subject<Loan[]>();
    const paymentsRequest = new Subject<LoanPayment[]>();
    loanService.loadLoans.and.returnValue(loansRequest);
    paymentService.loadPayments.and.returnValue(paymentsRequest);
    component.refresh();
    expect(component.isLoading).toBeTrue();
    loansRequest.next([testLoan]);
    loansRequest.complete();
    expect(component.isLoading).toBeTrue();
    paymentsRequest.next([payment]);
    paymentsRequest.complete();
    expect(component.isLoading).toBeFalse();
  });

  it('shows and clears the edit loader', () => {
    const request = new Subject<LoanPayment>();
    paymentService.getPayment.and.returnValue(request);
    component.startEdit(payment);
    expect(component.isLoadingPayment).toBeTrue();
    request.next(payment);
    request.complete();
    expect(component.isLoadingPayment).toBeFalse();
    expect(component.editing).toBeTrue();
  });

  it('loads unpaid installments and fills the selected EMI amount', () => {
    paymentService.getUnpaidInstallments.and.returnValue(of([{
      id: 11, loanId: 1, installmentNo: 1, dueDate: '2026-01-01',
      emiAmount: 5250, principalAmount: 5000, interestAmount: 250, outstandingBalance: 5000,
    }]));
    component.current.loanId = 1;
    component.onLoanChange();
    expect(component.unpaidInstallments.length).toBe(1);
    component.onScheduleChange(11);
    expect(component.current.amountPaid).toBe(5250);
  });

  it('prevents a payment before the installment due date', () => {
    component.unpaidInstallments = [{
      id: 11, loanId: 1, installmentNo: 1, dueDate: '2026-02-01',
      emiAmount: 5250, principalAmount: 5000, interestAmount: 250, outstandingBalance: 5000,
    }];
    component.current = { ...payment, id: 0, scheduleId: 11, paymentDate: '2026-01-01T10:00' };
    component.save({ invalid: false, control: { markAllAsTouched: () => undefined } } as any);
    expect(paymentService.createPayment).not.toHaveBeenCalled();
    expect(snackBar.open).toHaveBeenCalled();
  });

  it('creates a valid payment and exposes the saving state', () => {
    const request = new Subject<LoanPayment>();
    paymentService.createPayment.and.returnValue(request);
    component.unpaidInstallments = [{
      id: 11, loanId: 1, installmentNo: 1, dueDate: '2026-01-01',
      emiAmount: 5250, principalAmount: 5000, interestAmount: 250, outstandingBalance: 5000,
    }];
    component.current = { ...payment, id: 0, paymentDate: '2026-01-02T10:00' };
    component.save({ invalid: false, control: { markAllAsTouched: () => undefined } } as any);
    expect(component.isSaving).toBeTrue();
    request.next(payment);
    request.complete();
    expect(component.isSaving).toBeFalse();
    expect(snackBar.open).toHaveBeenCalledWith('Payment created successfully.', 'Close', jasmine.any(Object));
  });

  it('deletes a confirmed payment and exposes the deleting state', () => {
    const request = new Subject<any>();
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    paymentService.deletePayment.and.returnValue(request);
    component.remove(payment);
    expect(component.isDeleting).toBeTrue();
    request.next({});
    request.complete();
    expect(component.isDeleting).toBeFalse();
  });

  it('filters and clears payment search', () => {
    component.search = 'tx-5';
    expect(component.filteredPayments.length).toBe(1);
    component.clearSearch();
    expect(component.search).toBe('');
  });

  it('clears the loader and reports refresh failures', () => {
    loanService.loadLoans.and.returnValue(throwError(() => ({ error: { message: 'failed' } })));
    component.refresh();
    expect(component.isLoading).toBeFalse();
    expect(snackBar.open).toHaveBeenCalled();
  });
});
