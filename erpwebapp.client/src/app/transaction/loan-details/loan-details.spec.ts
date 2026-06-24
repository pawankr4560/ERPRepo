import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, Subject, throwError } from 'rxjs';
import { LoanDetailsComponent } from './loan-details';
import { Loan, LoanService } from '../services/loan-service';

describe('LoanDetailsComponent', () => {
  let component: LoanDetailsComponent;
  let fixture: ComponentFixture<LoanDetailsComponent>;
  let loanService: jasmine.SpyObj<LoanService>;
  let router: jasmine.SpyObj<Router>;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  const fullLoan: Loan = {
    id: 3,
    userId: 3,
    userName: 'Test Customer',
    loanNumber: 'LN-3',
    loanAmount: 100000,
    rate: 10,
    tenure: 2,
    emi: 52500,
    status: 'Active',
    startDate: '2026-01-01',
    endDate: '2026-03-01',
    emiSchedules: [
      { id: 1, loanId: 3, installmentNo: 1, dueDate: '2026-02-01', emiAmount: 52500, principalAmount: 50000, interestAmount: 2500, outstandingBalance: 50000, isPaid: true },
      { id: 2, loanId: 3, installmentNo: 2, dueDate: '2026-03-01', emiAmount: 52500, principalAmount: 50000, interestAmount: 2500, outstandingBalance: 0, isPaid: false },
    ],
    payments: [],
  };

  async function configure(id = '3', response = of(fullLoan)) {
    loanService = jasmine.createSpyObj<LoanService>('LoanService', ['getLoanById']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    snackBar = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);
    loanService.getLoanById.and.returnValue(response as any);

    await TestBed.configureTestingModule({
      imports: [LoanDetailsComponent],
      providers: [
        { provide: LoanService, useValue: loanService },
        { provide: Router, useValue: router },
        { provide: MatSnackBar, useValue: snackBar },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => id } } } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(LoanDetailsComponent);
    component = fixture.componentInstance;
    (component as any).router = router;
    (component as any).snackBar = snackBar;
  }

  it('shows a loader until the detail API completes', async () => {
    const request = new Subject<Loan>();
    await configure('3', request);
    fixture.detectChanges();
    expect(component.isLoading).toBeTrue();
    expect(fixture.nativeElement.querySelector('mat-spinner')).not.toBeNull();
    request.next(fullLoan);
    request.complete();
    fixture.detectChanges();
    expect(component.isLoading).toBeFalse();
    expect(fixture.nativeElement.querySelector('mat-spinner')).toBeNull();
  });

  it('loads details and calculates repayment totals', async () => {
    await configure();
    fixture.detectChanges();
    expect(component.loan?.loanNumber).toBe('LN-3');
    expect(component.paidCount).toBe(1);
    expect(component.paidProgress).toBe(50);
    expect(component.totalEmi).toBe(105000);
    expect(component.totalInterest).toBe(5000);
  });

  it('shows an error when the detail API fails', async () => {
    await configure('3', throwError(() => new Error('failed')));
    fixture.detectChanges();
    expect(component.isLoading).toBeFalse();
    expect(component.loadError).toContain('Unable to load');
  });

  it('rejects an invalid route id without calling the API', async () => {
    await configure('bad');
    fixture.detectChanges();
    expect(component.loadError).toBe('Invalid loan record.');
    expect(loanService.getLoanById).not.toHaveBeenCalled();
  });

  it('navigates back to the loan listing', async () => {
    await configure();
    fixture.detectChanges();
    component.backToLoans();
    expect(router.navigate).toHaveBeenCalledWith(['/home/inventory/transactions']);
  });

  it('opens a printable document for print and PDF export', async () => {
    await configure();
    fixture.detectChanges();
    const popup = {
      document: { open: jasmine.createSpy(), write: jasmine.createSpy(), close: jasmine.createSpy() },
      focus: jasmine.createSpy(),
      print: jasmine.createSpy(),
    } as any;
    spyOn(window, 'open').and.returnValue(popup);
    component.print();
    component.exportPdf();
    expect(window.open).toHaveBeenCalledTimes(2);
    expect(popup.document.write).toHaveBeenCalled();
    expect(popup.print).toHaveBeenCalledTimes(2);
  });

  it('warns when a popup is blocked', async () => {
    await configure();
    fixture.detectChanges();
    spyOn(window, 'open').and.returnValue(null);
    component.print();
    expect(snackBar.open).toHaveBeenCalled();
  });

  it('exports the persisted schedule as an Excel file', async () => {
    await configure();
    fixture.detectChanges();
    const download = spyOn<any>(component, 'downloadBlob');
    component.exportExcel();
    expect(download).toHaveBeenCalled();
    expect(download.calls.mostRecent().args[2]).toBe('LN-3-schedule.xls');
    expect(download.calls.mostRecent().args[0]).toContain('Installment No.');
  });
});
