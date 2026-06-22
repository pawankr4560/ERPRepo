import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { BookingDialog } from './booking-dialog';

describe('BookingDialog', () => {
  let ref: jasmine.SpyObj<MatDialogRef<BookingDialog>>;
  const cars = [
    { id: 1, brand: 'Honda', model: 'City', year: 2025, categoryId: 1, transmission: 'Automatic', fuelType: 'Petrol', seats: 5, pricePerDay: 1000, status: 'Available' },
    { id: 2, brand: 'Old', model: 'Car', year: 2020, categoryId: 1, transmission: 'Manual', fuelType: 'Diesel', seats: 5, pricePerDay: 800, status: 'Maintenance' },
  ];

  async function create(mode: 'create' | 'edit' | 'view' = 'create') {
    TestBed.resetTestingModule();
    ref = jasmine.createSpyObj('MatDialogRef', ['close']);
    await TestBed.configureTestingModule({
      imports: [BookingDialog],
      providers: [
        { provide: MatDialogRef, useValue: ref },
        { provide: MAT_DIALOG_DATA, useValue: {
          mode, cars, users: [{ id: 'u1', firstName: 'Test', lastName: 'User' }],
          booking: mode === 'create' ? undefined : {
            id: 4, bookingNumber: 'BK-4', userId: 'u1', userName: 'Test User',
            carId: 2, carName: 'Old Car', pickupDate: '2026-07-01', returnDate: '2026-07-03',
            totalDays: 2, amount: 1600, status: 'Pending', paymentStatus: 'Pending', createdDate: '2026-06-01',
          },
        } },
      ],
    }).compileComponents();
    return TestBed.createComponent(BookingDialog).componentInstance;
  }

  it('filters maintenance cars but retains the currently edited car', async () => {
    expect((await create()).selectableCars.map((x) => x.id)).toEqual([1]);
    expect((await create('edit')).selectableCars.map((x) => x.id)).toEqual([1, 2]);
  });

  it('calculates duration and amount', async () => {
    const component = await create();
    component.form.patchValue({ userId: 'u1', carId: 1, pickupDate: '2026-07-01', returnDate: '2026-07-04' });
    expect(component.estimatedDays).toBe(3);
    expect(component.estimatedAmount).toBe(3000);
  });

  it('validates reversed and overly long date ranges', async () => {
    const component = await create();
    component.form.patchValue({ pickupDate: '2026-07-05', returnDate: '2026-07-04' });
    expect(component.form.hasError('returnBeforePickup')).toBeTrue();
    component.form.patchValue({ pickupDate: '2026-01-01', returnDate: '2027-12-31' });
    expect(component.form.hasError('bookingTooLong')).toBeTrue();
  });

  it('closes with normalized dates and calculated totals', async () => {
    const component = await create();
    component.form.patchValue({ userId: 'u1', carId: 1, pickupDate: component.minDate });
    const returnDate = new Date(`${component.minDate}T00:00:00`);
    returnDate.setDate(returnDate.getDate() + 2);
    const returnValue = `${returnDate.getFullYear()}-${String(returnDate.getMonth() + 1).padStart(2, '0')}-${String(returnDate.getDate()).padStart(2, '0')}`;
    component.form.patchValue({ returnDate: returnValue });
    component.save();
    expect(ref.close).toHaveBeenCalledWith(jasmine.objectContaining({ carId: 1, totalDays: 2, amount: 2000 }));
  });

  it('disables view mode and closes with null', async () => {
    const component = await create('view');
    expect(component.form.disabled).toBeTrue();
    component.close();
    expect(ref.close).toHaveBeenCalledWith(null);
  });
});
