import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { Dashboard } from './dashboard';
import { DashboardService, LoanDashboardSummary } from './dashboard-service';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;
  let dashboardService: jasmine.SpyObj<DashboardService>;
  let router: jasmine.SpyObj<Router>;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  const summary: LoanDashboardSummary = {
    totalPortfolio: 1000000,
    totalCollected: 400000,
    outstandingPortfolio: 600000,
    overdueAmount: 25000,
    totalLoans: 10,
    activeLoans: 5,
    pendingLoans: 2,
    completedLoans: 2,
    otherLoans: 1,
    overdueInstallmentCount: 3,
    collectionRate: 40,
    creditScore: 762,
    creditScoreChange: 12,
    creditUtilization: 60,
    averageLoanAgeYears: 2.5,
    hardInquiries: 1,
    paymentHistoryRating: 'Good',
    queryDurationMs: 12,
    generatedAtUtc: '2026-06-22T00:00:00Z',
    activeLoanSummaries: [{
      loanId: 10,
      loanNumber: 'LN-10',
      customerName: 'Upcoming Customer',
      loanAmount: 100000,
      rate: 10,
      emi: 12000,
      tenureMonths: 12,
      paidInstallments: 4,
      totalInstallments: 12,
      monthsRemaining: 8,
      outstandingBalance: 60000,
      progressPercentage: 33,
    }],
    upcomingInstallments: [{
      id: 1,
      loanId: 10,
      loanNumber: 'LN-10',
      customerName: 'Upcoming Customer',
      installmentNo: 2,
      dueDate: '2026-07-01',
      emiAmount: 12000,
    }],
    recentPayments: [{
      id: 2,
      loanId: 11,
      loanNumber: 'LN-11',
      customerName: 'Recent Customer',
      amountPaid: 9000,
      paymentDate: '2026-06-20',
      paymentStatus: 'Success',
      transactionId: 'TX-11',
    }],
  };

  beforeEach(async () => {
    dashboardService = jasmine.createSpyObj<DashboardService>('DashboardService', ['loadSummary']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);
    snackBar = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);
    dashboardService.loadSummary.and.returnValue(of(summary));

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        { provide: DashboardService, useValue: dashboardService },
        { provide: Router, useValue: router },
        { provide: MatSnackBar, useValue: snackBar },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    (component as any).router = router;
    (component as any).snackBar = snackBar;
    fixture.detectChanges();
  });

  it('loads and displays dashboard summary on initialization', () => {
    expect(dashboardService.loadSummary).toHaveBeenCalled();
    expect(component.summary).toEqual(summary);
    expect(component.totalPortfolio).toBe(1000000);
    expect(fixture.nativeElement.textContent).toContain('Upcoming Customer');
    expect(fixture.nativeElement.textContent).toContain('Recent Customer');
  });

  it('shows the loader until the summary API completes', () => {
    const request = new Subject<LoanDashboardSummary>();
    dashboardService.loadSummary.and.returnValue(request);

    component.refresh();
    fixture.detectChanges();
    expect(component.isLoading).toBeTrue();
    expect(fixture.nativeElement.querySelector('.loading-state')).not.toBeNull();
    expect(fixture.nativeElement.querySelector('.loading-state').getAttribute('role')).toBe('status');

    request.next(summary);
    request.complete();
    fixture.detectChanges();
    expect(component.isLoading).toBeFalse();
    expect(fixture.nativeElement.querySelector('.loading-state')).toBeNull();
  });

  it('clears the loader and shows the API error message on failure', () => {
    dashboardService.loadSummary.and.returnValue(
      throwError(() => ({ error: { message: 'Dashboard unavailable' } }))
    );
    component.refresh();
    expect(component.isLoading).toBeFalse();
    expect(snackBar.open).toHaveBeenCalledWith(
      'Dashboard unavailable',
      'Close',
      jasmine.objectContaining({ duration: 4500 })
    );
  });

  it('uses the fallback error message when the API has no message', () => {
    dashboardService.loadSummary.and.returnValue(throwError(() => new Error('failed')));
    component.refresh();
    expect(snackBar.open).toHaveBeenCalledWith(
      'Unable to load loan dashboard.',
      'Close',
      jasmine.any(Object)
    );
  });

  it('calculates each loan status percentage from total loans', () => {
    expect(component.loanStatusBreakdown.map((item) => item.percentage)).toEqual([50, 20, 20, 10]);
  });

  it('returns zero percentages when no loans exist', () => {
    component.summary = { ...summary, totalLoans: 0 };
    expect(component.loanStatusBreakdown.every((item) => item.percentage === 0)).toBeTrue();
  });

  it('navigates to loans, payments, and menu management', () => {
    component.navigate('loans');
    component.navigate('payments');
    component.navigate('menu');
    expect(router.navigate).toHaveBeenCalledWith(['/home/inventory/transactions']);
    expect(router.navigate).toHaveBeenCalledWith(['/home/inventory/payments']);
    expect(router.navigate).toHaveBeenCalledWith(['/home/menu']);
  });

  it('creates normalized payment status CSS classes', () => {
    expect(component.paymentStatusClass('Success')).toBe('status-success');
    expect(component.paymentStatusClass('')).toBe('status-pending');
  });

  it('renders empty states when there are no installments or payments', () => {
    component.summary = {
      ...summary,
      upcomingInstallments: [],
      recentPayments: [],
    };
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No installments are due in the next 30 days.');
    expect(fixture.nativeElement.textContent).toContain('No loan payments have been recorded yet.');
  });

  it('stops the pending API subscription when destroyed', () => {
    const request = new Subject<LoanDashboardSummary>();
    dashboardService.loadSummary.and.returnValue(request);
    component.refresh();
    expect(request.observed).toBeTrue();
    component.ngOnDestroy();
    expect(request.observed).toBeFalse();
    expect(component.isLoading).toBeFalse();
  });
});
