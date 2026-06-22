import { TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { BookingPaymentComponent } from './booking-payment';
import { BookingService } from '../../booking/services/booking-service';
import { BookingPaymentService } from '../services/booking-payment-service';
import { Booking } from '../../booking/interfaces/booking';
import { BookingPayment } from '../interfaces/booking-payment';

describe('BookingPaymentComponent', () => {
  let component: BookingPaymentComponent;
  let bookingService: jasmine.SpyObj<BookingService>;
  let paymentService: jasmine.SpyObj<BookingPaymentService>;
  let dialog: jasmine.SpyObj<MatDialog>;
  let snack: jasmine.SpyObj<MatSnackBar>;
  const booking: Booking = { id: 2, bookingNumber: 'BK-2', userId: 'u1', userName: 'User', carId: 1, carName: 'Honda', pickupDate: '2026-07-01', returnDate: '2026-07-03', totalDays: 2, amount: 2000, status: 'Confirmed', paymentStatus: 'Pending', createdDate: '2026-06-01' };
  const payment: BookingPayment = { id: 1, bookingId: 2, bookingNumber: 'BK-2', customerName: 'User', bookingAmount: 2000, amount: 500, paymentDate: '2026-06-22', paymentMethod: 'UPI', status: 'Paid', transactionReference: 'TX-1', notes: null, createdDate: '2026-06-22' };

  beforeEach(async () => {
    bookingService = jasmine.createSpyObj('BookingService', ['loadBookings'], { bookings$: new BehaviorSubject([booking]).asObservable() });
    paymentService = jasmine.createSpyObj('BookingPaymentService', ['loadPayments', 'createPayment', 'updatePayment', 'deletePayment'], { payments$: new BehaviorSubject([payment]).asObservable() });
    dialog = jasmine.createSpyObj('MatDialog', ['open']); snack = jasmine.createSpyObj('MatSnackBar', ['open']);
    bookingService.loadBookings.and.returnValue(of([booking])); paymentService.loadPayments.and.returnValue(of([payment]));
    await TestBed.configureTestingModule({
      imports: [BookingPaymentComponent],
      providers: [
        { provide: BookingService, useValue: bookingService }, { provide: BookingPaymentService, useValue: paymentService },
        { provide: MatDialog, useValue: dialog }, { provide: MatSnackBar, useValue: snack },
        { provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: { get: () => '2' } } } },
      ],
    }).compileComponents();
    component = TestBed.createComponent(BookingPaymentComponent).componentInstance;
    (component as any).dialog = dialog; (component as any).snackBar = snack;
    component.ngOnInit();
  });

  it('loads the selected booking and payments from query parameters', () => {
    expect(component.selectedBookingId).toBe(2);
    expect(paymentService.loadPayments).toHaveBeenCalledWith(2);
    expect(component.editing).toBeTrue();
  });

  it('keeps refresh loading until both requests complete', () => {
    const bookings = new Subject<Booking[]>(); const payments = new Subject<BookingPayment[]>();
    bookingService.loadBookings.and.returnValue(bookings); paymentService.loadPayments.and.returnValue(payments);
    component.refresh();
    expect(component.isLoading).toBeTrue();
    bookings.next([booking]); bookings.complete();
    expect(component.isLoading).toBeTrue();
    payments.next([payment]); payments.complete();
    expect(component.isLoading).toBeFalse();
  });

  it('calculates outstanding amount excluding the edited payment', () => {
    component.current = { ...payment };
    expect(component.outstandingAmount).toBe(2000);
    component.current = { ...payment, id: 0 };
    expect(component.outstandingAmount).toBe(1500);
  });

  it('prevents overpayment', () => {
    component.current = { ...payment, id: 0, amount: 3000 };
    component.save({ invalid: false, control: { markAllAsTouched: () => undefined } } as any);
    expect(paymentService.createPayment).not.toHaveBeenCalled();
    expect(snack.open).toHaveBeenCalled();
  });

  it('creates and deletes payments with busy states', () => {
    const create = new Subject<BookingPayment>();
    paymentService.createPayment.and.returnValue(create);
    component.current = { ...payment, id: 0, amount: 1500 };
    component.save({ invalid: false, control: { markAllAsTouched: () => undefined } } as any);
    expect(component.isSaving).toBeTrue();
    create.next(payment); create.complete();
    expect(component.isSaving).toBeFalse();

    const remove = new Subject<void>();
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    paymentService.deletePayment.and.returnValue(remove);
    component.remove(payment);
    expect(component.isDeleting).toBeTrue();
    remove.next(); remove.complete();
    expect(component.isDeleting).toBeFalse();
  });

  it('filters payments and builds status classes', () => {
    component.search = 'tx-1';
    expect(component.filteredPayments.length).toBe(1);
    expect(component.statusClass('Refunded')).toBe('status-refunded');
  });
});
