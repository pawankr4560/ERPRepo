import { BreakpointObserver } from '@angular/cdk/layout';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { BookingMaster } from './booking-master';
import { BookingService } from '../services/booking-service';
import { Booking } from '../interfaces/booking';

describe('BookingMaster', () => {
  let component: BookingMaster;
  let service: jasmine.SpyObj<BookingService>;
  let dialog: jasmine.SpyObj<MatDialog>;
  let snack: jasmine.SpyObj<MatSnackBar>;
  let router: jasmine.SpyObj<Router>;
  const bookings$ = new BehaviorSubject<Booking[]>([]);
  const booking: Booking = { id: 1, bookingNumber: 'BK-1', userId: 'u1', userName: 'User', carId: 2, carName: 'Honda City', pickupDate: '2026-07-01', returnDate: '2026-07-03', totalDays: 2, amount: 2000, status: 'Pending', paymentStatus: 'Pending', createdDate: '2026-06-01' };
  const options = { cars: [{ id: 2, brand: 'Honda', model: 'City', year: 2025, categoryId: 1, transmission: 'Automatic', fuelType: 'Petrol', seats: 5, pricePerDay: 1000, status: 'Available' }], users: [{ id: 'u1', firstName: 'Test', lastName: 'User' }] };

  beforeEach(async () => {
    service = jasmine.createSpyObj('BookingService', ['loadDialogOptions', 'loadBookings', 'createBooking', 'updateBooking', 'deleteBooking'], { bookings$: bookings$.asObservable() });
    dialog = jasmine.createSpyObj('MatDialog', ['open']); snack = jasmine.createSpyObj('MatSnackBar', ['open']); router = jasmine.createSpyObj('Router', ['navigate']);
    service.loadDialogOptions.and.returnValue(of(options)); service.loadBookings.and.returnValue(of([booking]));
    await TestBed.configureTestingModule({
      imports: [BookingMaster],
      providers: [
        { provide: BookingService, useValue: service }, { provide: MatDialog, useValue: dialog },
        { provide: MatSnackBar, useValue: snack }, { provide: Router, useValue: router },
        { provide: BreakpointObserver, useValue: { observe: () => of({ matches: false }) } },
      ],
    }).compileComponents();
    component = TestBed.createComponent(BookingMaster).componentInstance;
    (component as any).dialog = dialog; (component as any).snackBar = snack; (component as any).router = router;
    component.ngOnInit();
  });

  it('loads options and bookings in sequence', () => {
    expect(service.loadDialogOptions).toHaveBeenCalled();
    expect(service.loadBookings).toHaveBeenCalled();
    expect(component.cars.length).toBe(1);
    expect(component.isBusy).toBeFalse();
  });

  it('shows loading while create options are pending', () => {
    const request = new Subject<any>();
    service.loadDialogOptions.and.returnValue(request);
    dialog.open.and.returnValue({ afterClosed: () => of(null) } as any);
    component.createBooking();
    expect(component.isLoading).toBeTrue();
    request.next(options); request.complete();
    expect(component.isLoading).toBeFalse();
  });

  it('blocks creation when users or cars are unavailable', () => {
    service.loadDialogOptions.and.returnValue(of({ cars: [], users: [] }));
    component.createBooking();
    expect(dialog.open).not.toHaveBeenCalled();
    expect(snack.open).toHaveBeenCalled();
  });

  it('creates a booking and exposes mutation loading', () => {
    const request = new Subject<Booking>();
    service.createBooking.and.returnValue(request);
    dialog.open.and.returnValue({ afterClosed: () => of(booking) } as any);
    component.createBooking();
    expect(component.isMutating).toBeTrue();
    request.next(booking); request.complete();
    expect(component.isMutating).toBeFalse();
  });

  it('deletes a confirmed booking and reports API errors', () => {
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    service.deleteBooking.and.returnValue(throwError(() => ({ error: { message: 'Delete blocked' } })));
    component.deleteBooking(booking);
    expect(component.isMutating).toBeFalse();
    expect(snack.open).toHaveBeenCalledWith('Delete blocked', 'Close', jasmine.any(Object));
  });

  it('navigates to the selected booking payments', () => {
    component.managePayments(booking);
    expect(router.navigate).toHaveBeenCalledWith(['/home/booking/payments'], { queryParams: { bookingId: 1 } });
  });
});
