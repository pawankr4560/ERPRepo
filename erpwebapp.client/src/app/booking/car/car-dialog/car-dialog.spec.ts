import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CarDialog } from './car-dialog';

describe('CarDialog', () => {
  let ref: jasmine.SpyObj<MatDialogRef<CarDialog>>;

  async function create(mode: 'create' | 'edit' | 'view' = 'create') {
    ref = jasmine.createSpyObj('MatDialogRef', ['close']);
    await TestBed.configureTestingModule({
      imports: [CarDialog],
      providers: [
        { provide: MatDialogRef, useValue: ref },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            mode,
            categories: [{ id: 1, name: 'SUV' }],
            car: mode === 'create' ? undefined : {
              id: 2, brand: 'Toyota', model: 'Fortuner', year: 2025,
              categoryId: 1, transmission: 'Automatic', fuelType: 'Diesel',
              seats: 7, pricePerDay: 5000, imageUrl: 'https://example.com/car.jpg',
              status: 'Available',
            },
          },
        },
      ],
    }).compileComponents();
    return TestBed.createComponent(CarDialog).componentInstance;
  }

  it('validates required fields and does not close an invalid form', async () => {
    const component = await create();
    component.save();
    expect(component.form.invalid).toBeTrue();
    expect(ref.close).not.toHaveBeenCalled();
  });

  it('closes with a valid car payload', async () => {
    const component = await create();
    component.form.patchValue({
      brand: 'Honda', model: 'City', categoryId: 1, transmission: 'Automatic',
      fuelType: 'Petrol', seats: 5, pricePerDay: 2500, imageUrl: 'https://example.com/honda.jpg',
    });
    component.save();
    expect(ref.close).toHaveBeenCalledWith(jasmine.objectContaining({ brand: 'Honda', categoryId: 1 }));
  });

  it('disables the form in view mode and closes with null', async () => {
    const component = await create('view');
    expect(component.form.disabled).toBeTrue();
    component.close();
    expect(ref.close).toHaveBeenCalledWith(null);
  });

  it('rejects invalid image URLs, years, seats, and prices', async () => {
    const component = await create();
    component.form.patchValue({ imageUrl: 'invalid', year: 1800, seats: 0, pricePerDay: 0 });
    expect(component.form.get('imageUrl')?.hasError('pattern')).toBeTrue();
    expect(component.form.get('year')?.invalid).toBeTrue();
    expect(component.form.get('seats')?.invalid).toBeTrue();
    expect(component.form.get('pricePerDay')?.invalid).toBeTrue();
  });
});
