import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { LoanComponent } from './loan-component';
import { Loan, LoanService } from '../services/loan-service';
import { InterestSettingService } from '../../setting/interest-setting.service';
import { LoanPaymentService } from '../services/loan-payment-service';

describe('LoanComponent', () => {
  let component: LoanComponent;
  let fixture: ComponentFixture<LoanComponent>;
  let loans$: BehaviorSubject<Loan[]>;
  let loanService: jasmine.SpyObj<LoanService>;
  let interestService: jasmine.SpyObj<InterestSettingService>;
  let router: jasmine.SpyObj<Router>;
  let dialog: jasmine.SpyObj<MatDialog>;
  let snackBar: jasmine.SpyObj<MatSnackBar>;
  let loanPaymentService: jasmine.SpyObj<LoanPaymentService>;

  const loan = (id: number, number = `LN-${id}`): Loan => ({
    id,
    loanNumber: number,
    userId: `user-${id}`,
    userName: `Customer ${id}`,
    loanAmount: 120000,
    rate: 12,
    tenure: 12,
    emi: 11000,
    status: 'Active',
    startDate: '2026-01-01',
    endDate: '2027-01-01',
  });

  beforeEach(async () => {
    loans$ = new BehaviorSubject<Loan[]>([]);
    loanService = jasmine.createSpyObj<LoanService>(
      'LoanService',
      [
        'loadLoans',
        'getLoanData',
        'createLoan',
        'updateLoan',
        'deleteLoan',
        'approveLoan',
        'rejectLoan',
        'getScheduleByLoanNumber',
      ],
      { loans$: loans$.asObservable() }
    );
    interestService = jasmine.createSpyObj<InterestSettingService>('InterestSettingService', ['load', 'calculateEmi']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    dialog = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    snackBar = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);
    loanPaymentService = jasmine.createSpyObj<LoanPaymentService>('LoanPaymentService', ['loadPayments']);

    loanService.loadLoans.and.returnValue(of([]));
    loanService.getLoanData.and.returnValue(of({ loanNumber: '2026-GKFIN-00001', customerList: [] }));
    loanPaymentService.loadPayments.and.returnValue(of([]));
    interestService.load.and.returnValue(of('Reducing'));
    interestService.calculateEmi.and.callFake((amount, _rate, months) => months ? amount / months : 0);

    await TestBed.configureTestingModule({
      imports: [LoanComponent],
      providers: [
        { provide: LoanService, useValue: loanService },
        { provide: InterestSettingService, useValue: interestService },
        { provide: Router, useValue: router },
        { provide: MatDialog, useValue: dialog },
        { provide: MatSnackBar, useValue: snackBar },
        { provide: LoanPaymentService, useValue: loanPaymentService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoanComponent);
    component = fixture.componentInstance;
    (component as any).dialog = dialog;
    (component as any).snackBar = snackBar;
    (component as any).router = router;
    fixture.detectChanges();
  });

  it('loads loans and the interest setting on initialization', () => {
    expect(loanService.loadLoans).toHaveBeenCalled();
    expect(interestService.load).toHaveBeenCalled();
    expect(component.systemInterestType).toBe('Reducing');
  });

  it('keeps the loader visible until the loan list API completes', () => {
    const request = new Subject<Loan[]>();
    loanService.loadLoans.and.returnValue(request);

    component.load();
    fixture.detectChanges();
    expect(component.isLoading).toBeTrue();
    expect(fixture.nativeElement.querySelector('.api-loader-overlay')).not.toBeNull();

    request.next([]);
    request.complete();
    fixture.detectChanges();
    expect(component.isLoading).toBeFalse();
    expect(fixture.nativeElement.querySelector('.api-loader-overlay')).toBeNull();
  });

  it('reports list API errors and clears the loader', () => {
    loanService.loadLoans.and.returnValue(throwError(() => ({ error: { message: 'Network failed' } })));
    component.load();
    expect(component.isLoading).toBeFalse();
    expect(snackBar.open).toHaveBeenCalledWith('Network failed', 'Close', jasmine.any(Object));
  });

  it('filters loans and resets pagination when search changes', () => {
    component.loans = [loan(1, 'ALPHA'), loan(2, 'BETA')];
    component.pageIndex = 2;
    component.onSearchChange('customer 2');
    expect(component.pageIndex).toBe(0);
    expect(component.filteredLoans.map((item) => item.loanNumber)).toEqual(['BETA']);
  });

  it('paginates loan records', () => {
    component.loans = Array.from({ length: 21 }, (_, index) => loan(index + 1));
    component.nextPage();
    expect(component.currentPageNumber).toBe(2);
    expect(component.pagedLoans.length).toBe(10);
    component.lastPage();
    expect(component.currentPageNumber).toBe(3);
    expect(component.pagedLoans.length).toBe(1);
  });

  it('opens the dedicated details route for a row', () => {
    component.openLoanDetails(loan(7));
    expect(router.navigate).toHaveBeenCalledWith(['/home/inventory/transactions', 7]);
  });

  it('loads form data and shows the loader when creating a loan', () => {
    const request = new Subject<any>();
    loanService.getLoanData.and.returnValue(request);
    component.startCreate();
    expect(component.isLoadingLoanData).toBeTrue();
    request.next({ loanNumber: '2026-GKFIN-00009', customerList: [{ id: 'u1', customerName: 'A User' }] });
    request.complete();
    expect(component.isLoadingLoanData).toBeFalse();
    expect(component.current.loanNumber).toBe('2026-GKFIN-00009');
    expect(component.customers.length).toBe(1);
  });

  it('creates a valid loan and keeps the saving loader active during the API call', () => {
    const request = new Subject<any>();
    loanService.createLoan.and.returnValue(request);
    spyOn(component, 'isWizardValid').and.returnValue(true);
    component.current = {
      ...loan(0),
      id: 0,
      userId: 'u1',
      loanNumber: 'LN-NEW',
      loanAmount: 120000,
      rate: 12,
      tenure: 12,
      startDate: '2026-01-01',
      interestCalculationType: 'Reducing',
    };

    component.saveWizard();
    expect(component.isSaving).toBeTrue();
    expect(loanService.createLoan).toHaveBeenCalled();
    request.next({});
    request.complete();
    expect(component.isSaving).toBeFalse();
    expect(snackBar.open).toHaveBeenCalledWith('Loan created successfully.', 'Close', jasmine.any(Object));
  });

  it('deletes a confirmed loan and exposes the deleting state', () => {
    const request = new Subject<any>();
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    loanService.deleteLoan.and.returnValue(request);

    component.remove({ ...loan(4), status: 'Pending', active: false });
    expect(component.isDeleting).toBeTrue();
    expect(loanService.deleteLoan).toHaveBeenCalledWith(4);
    request.next({});
    request.complete();
    expect(component.isDeleting).toBeFalse();
  });

  it('approves a pending loan after confirmation', () => {
    const pendingLoan = { ...loan(5), status: 'Pending', active: false };
    const request = new Subject<any>();
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    loanService.approveLoan.and.returnValue(request);

    component.approve(pendingLoan);

    expect(component.approvalActionLoanId).toBe(5);
    expect(loanService.approveLoan).toHaveBeenCalledWith(5);
    request.next({ message: 'Loan approved successfully.' });
    request.complete();
    expect(component.approvalActionLoanId).toBeNull();
    expect(loanService.loadLoans).toHaveBeenCalled();
  });

  it('rejects a pending loan after confirmation', () => {
    const pendingLoan = { ...loan(6), status: 'Pending', active: false };
    const request = new Subject<any>();
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    loanService.rejectLoan.and.returnValue(request);

    component.reject(pendingLoan);

    expect(component.approvalActionLoanId).toBe(6);
    expect(loanService.rejectLoan).toHaveBeenCalledWith(6);
    request.next({ message: 'Loan rejected successfully.' });
    request.complete();
    expect(component.approvalActionLoanId).toBeNull();
    expect(loanService.loadLoans).toHaveBeenCalled();
  });

  it('does not open edit mode for an approved loan', () => {
    const approvedLoan = { ...loan(7), status: 'Active', active: true };

    component.startEdit(approvedLoan);

    expect(component.editing).toBeFalse();
  });

  it('does not delete an approved loan', () => {
    const approvedLoan = { ...loan(8), status: 'Active', active: true };

    component.remove(approvedLoan);

    expect(loanService.deleteLoan).not.toHaveBeenCalled();
  });

  it('calculates EMI schedule totals consistently', () => {
    component.current = {
      ...loan(1),
      loanAmount: 120000,
      rate: 12,
      tenure: 12,
      startDate: '2026-01-01',
      interestCalculationType: 'Reducing',
    };
    expect(component.emiPreview.length).toBe(12);
    expect(component.previewTotalPayable).toBeGreaterThan(0);
    expect(component.previewTotalInterest).toBeGreaterThanOrEqual(0);
  });

  it('starts the EMI preview from the next month on the same date', () => {
    component.current = {
      ...loan(1),
      loanAmount: 120000,
      rate: 12,
      tenure: 12,
      startDate: '2026-01-01',
      interestCalculationType: 'Reducing',
    };

    const firstDueDate = component.emiPreview[0].dueDate;
    expect(firstDueDate.getFullYear()).toBe(2026);
    expect(firstDueDate.getMonth()).toBe(1);
    expect(firstDueDate.getDate()).toBe(1);
  });

  it('keeps next-month due dates valid when the same day does not exist', () => {
    component.current = {
      ...loan(1),
      loanAmount: 120000,
      rate: 12,
      tenure: 12,
      startDate: '2026-01-31',
      interestCalculationType: 'Flat',
    };

    const firstDueDate = component.emiPreview[0].dueDate;
    expect(firstDueDate.getFullYear()).toBe(2026);
    expect(firstDueDate.getMonth()).toBe(1);
    expect(firstDueDate.getDate()).toBe(28);
  });

  it('accepts a six-month tenure and rejects shorter tenure', () => {
    component.current = {
      ...loan(1),
      tenure: 6,
      startDate: '2026-01-01',
      interestCalculationType: 'Reducing',
    };

    expect(component.tenureYears).toBe(0.5);
    expect(component.isLoanStepValid()).toBeTrue();

    component.current.tenure = 5;
    expect(component.isLoanStepValid()).toBeFalse();
  });
});
