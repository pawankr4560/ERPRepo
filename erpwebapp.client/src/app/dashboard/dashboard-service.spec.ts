import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { DashboardService } from './dashboard-service';
import { environment } from '../environments/environment';

describe('DashboardService', () => {
  let service: DashboardService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(DashboardService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('requests the loan summary with the configured API key', () => {
    service.loadSummary().subscribe();
    const request = http.expectOne(`${environment.apiUrl}/api/Dashboard/loan-summary`);
    expect(request.request.method).toBe('GET');
    expect(request.request.headers.get('api_key')).toBe(environment.apiKey);
    request.flush({});
  });

  it('normalizes PascalCase API responses and nested rows', () => {
    let result: any;
    service.loadSummary().subscribe((summary) => (result = summary));
    const request = http.expectOne(`${environment.apiUrl}/api/Dashboard/loan-summary`);
    request.flush({
      data: {
        TotalPortfolio: 900,
        TotalCollected: 300,
        OutstandingPortfolio: 600,
        OverdueAmount: 50,
        TotalLoans: 4,
        ActiveLoans: 2,
        PendingLoans: 1,
        CompletedLoans: 1,
        OtherLoans: 0,
        OverdueInstallmentCount: 1,
        CollectionRate: 33.3,
        QueryDurationMs: 8,
        GeneratedAtUtc: '2026-06-22T00:00:00Z',
        UpcomingInstallments: [{
          Id: 1,
          LoanId: 2,
          LoanNumber: 'LN-2',
          CustomerName: 'Customer',
          InstallmentNo: 3,
          DueDate: '2026-07-01',
          EmiAmount: 100,
        }],
        RecentPayments: [{
          Id: 4,
          LoanId: 2,
          LoanNumber: 'LN-2',
          CustomerName: 'Customer',
          AmountPaid: 100,
          PaymentDate: '2026-06-20',
          PaymentStatus: 'Success',
          TransactionId: 'TX-4',
        }],
      },
    });

    expect(result.totalPortfolio).toBe(900);
    expect(result.upcomingInstallments[0].installmentNo).toBe(3);
    expect(result.recentPayments[0].transactionId).toBe('TX-4');
  });

  it('provides safe defaults for an empty response', () => {
    let result: any;
    service.loadSummary().subscribe((summary) => (result = summary));
    http.expectOne(`${environment.apiUrl}/api/Dashboard/loan-summary`).flush(null);
    expect(result.totalPortfolio).toBe(0);
    expect(result.collectionRate).toBe(0);
    expect(result.upcomingInstallments).toEqual([]);
    expect(result.recentPayments).toEqual([]);
  });

  it('passes HTTP errors to the component', () => {
    let status = 0;
    service.loadSummary().subscribe({
      error: (error) => (status = error.status),
    });
    http.expectOne(`${environment.apiUrl}/api/Dashboard/loan-summary`)
      .flush({ message: 'failed' }, { status: 500, statusText: 'Server Error' });
    expect(status).toBe(500);
  });
});
