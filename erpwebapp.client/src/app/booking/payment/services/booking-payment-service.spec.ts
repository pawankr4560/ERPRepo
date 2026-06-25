import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { BookingPaymentService } from './booking-payment-service';

describe('BookingPaymentService', () => {
  let service: BookingPaymentService;
  let http: HttpTestingController;
  const payment = { id: 1, bookingId: 2, bookingNumber: 'BK-2', customerName: 'User', bookingAmount: 2000, amount: 1000, paymentDate: '2026-06-22', paymentMethod: 'UPI', status: 'Paid', transactionReference: 'TX-1', notes: null, createdDate: '2026-06-22' };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [BookingPaymentService, provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(BookingPaymentService); http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('loads filtered payments and normalizes responses', () => {
    service.loadPayments(2).subscribe((items) => expect(items[0].transactionReference).toBe('TX-1'));
    http.expectOne(`${environment.apiUrl}/BookingPayment?bookingId=2`).flush({ Data: [{ ...payment, TransactionReference: 'TX-1', transactionReference: undefined }] });
  });

  it('creates, updates, and deletes payment store entries', () => {
    service.createPayment(payment).subscribe();
    const create = http.expectOne(`${environment.apiUrl}/BookingPayment`);
    expect(create.request.body.bookingId).toBe(2);
    create.flush(payment);
    service.updatePayment({ ...payment, amount: 1500 }).subscribe();
    http.expectOne(`${environment.apiUrl}/BookingPayment/1`).flush({ ...payment, amount: 1500 });
    service.deletePayment(1).subscribe();
    http.expectOne(`${environment.apiUrl}/BookingPayment/1`).flush(null);
    let stored: any[] = [];
    service.payments$.subscribe((items) => (stored = items));
    expect(stored).toEqual([]);
  });
});
