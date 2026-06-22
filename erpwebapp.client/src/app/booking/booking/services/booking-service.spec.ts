import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { BookingService } from './booking-service';

describe('BookingService', () => {
  let service: BookingService;
  let http: HttpTestingController;
  const booking = { id: 1, bookingNumber: 'BK-1', userId: 'u1', userName: 'User', carId: 2, carName: 'Honda City', pickupDate: '2026-07-01', returnDate: '2026-07-03', totalDays: 2, amount: 2000, status: 'Pending', paymentStatus: 'Pending', createdDate: '2026-06-01' };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [BookingService, provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(BookingService); http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('normalizes and stores booking list responses', () => {
    let result: any[] = [];
    service.bookings$.subscribe((x) => (result = x));
    service.loadBookings().subscribe();
    http.expectOne(`${environment.apiUrl}/api/Booking`).flush({ Data: [{ ...booking, Id: 1, id: undefined }] });
    expect(result[0].id).toBe(1);
  });

  it('normalizes dialog cars and users', () => {
    service.loadDialogOptions().subscribe((options) => {
      expect(options.cars[0].brand).toBe('Honda');
      expect(options.users[0].firstName).toBe('Test');
    });
    http.expectOne(`${environment.apiUrl}/api/Booking/options`).flush({
      Data: { Cars: [{ Id: 2, Brand: 'Honda', Model: 'City', PricePerDay: 1000, Status: 'Available' }], Users: [{ Id: 'u1', FirstName: 'Test', LastName: 'User' }] },
    });
  });

  it('creates, updates, and deletes bookings while maintaining the store', () => {
    service.createBooking(booking).subscribe();
    const create = http.expectOne(`${environment.apiUrl}/api/Booking`);
    expect(create.request.body).toEqual({ carId: 2, userId: 'u1', pickupDate: '2026-07-01', returnDate: '2026-07-03' });
    create.flush(booking);
    service.updateBooking({ ...booking, status: 'Confirmed' }).subscribe();
    const update = http.expectOne(`${environment.apiUrl}/api/Booking/1`);
    expect(update.request.body.status).toBe('Confirmed');
    update.flush({ ...booking, status: 'Confirmed' });
    service.deleteBooking(1).subscribe();
    http.expectOne(`${environment.apiUrl}/api/Booking/1`).flush(null);
    let stored: any[] = [];
    service.bookings$.subscribe((x) => (stored = x));
    expect(stored).toEqual([]);
  });
});
