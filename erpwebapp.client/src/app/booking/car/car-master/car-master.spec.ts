import { BreakpointObserver } from '@angular/cdk/layout';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BehaviorSubject, of, Subject, throwError } from 'rxjs';
import { CarMaster } from './car-master';
import { CarService } from '../services/car-service';
import { Car } from '../interfaces/car';

describe('CarMaster', () => {
  let component: CarMaster;
  let service: jasmine.SpyObj<CarService>;
  let dialog: jasmine.SpyObj<MatDialog>;
  let snack: jasmine.SpyObj<MatSnackBar>;
  const cars$ = new BehaviorSubject<Car[]>([]);
  const car: Car = { id: 1, brand: 'Honda', model: 'City', year: 2025, categoryId: 1, transmission: 'Automatic', fuelType: 'Petrol', seats: 5, pricePerDay: 2500, status: 'Available' };

  beforeEach(async () => {
    service = jasmine.createSpyObj('CarService', ['loadCategories', 'loadCars', 'createCar', 'updateCar', 'deleteCar'], { cars$: cars$.asObservable() });
    dialog = jasmine.createSpyObj('MatDialog', ['open']);
    snack = jasmine.createSpyObj('MatSnackBar', ['open']);
    service.loadCategories.and.returnValue(of([{ id: 1, name: 'SUV' }]));
    service.loadCars.and.returnValue(of([car]));
    await TestBed.configureTestingModule({
      imports: [CarMaster],
      providers: [
        { provide: CarService, useValue: service }, { provide: MatDialog, useValue: dialog },
        { provide: MatSnackBar, useValue: snack },
        { provide: BreakpointObserver, useValue: { observe: () => of({ matches: false }) } },
      ],
    }).compileComponents();
    component = TestBed.createComponent(CarMaster).componentInstance;
    (component as any).dialog = dialog; (component as any).snackBar = snack;
    component.ngOnInit();
  });

  it('loads categories then cars and clears the loader', () => {
    expect(service.loadCategories).toHaveBeenCalled();
    expect(service.loadCars).toHaveBeenCalled();
    expect(component.categories.length).toBe(1);
    expect(component.isBusy).toBeFalse();
  });

  it('keeps loading visible until both APIs complete', () => {
    const categories = new Subject<any[]>();
    service.loadCategories.and.returnValue(categories);
    (component as any).loadData();
    expect(component.isLoading).toBeTrue();
    categories.next([{ id: 1, name: 'SUV' }]);
    const cars = new Subject<Car[]>();
    service.loadCars.and.returnValue(cars);
    categories.complete();
    // switchMap subscribed before completion; use a fresh run for deterministic delayed cars.
    service.loadCategories.and.returnValue(of([{ id: 1, name: 'SUV' }]));
    service.loadCars.and.returnValue(cars);
    (component as any).loadData();
    expect(component.isLoading).toBeTrue();
    cars.next([]); cars.complete();
    expect(component.isLoading).toBeFalse();
  });

  it('blocks create when categories are unavailable', () => {
    component.categories = [];
    component.openCreateDialog();
    expect(dialog.open).not.toHaveBeenCalled();
    expect(snack.open).toHaveBeenCalled();
  });

  it('creates a car and exposes mutation loading', () => {
    component.categories = [{ id: 1, name: 'SUV' }];
    const request = new Subject<Car>();
    service.createCar.and.returnValue(request);
    dialog.open.and.returnValue({ afterClosed: () => of(car) } as any);
    component.openCreateDialog();
    expect(component.isMutating).toBeTrue();
    request.next(car); request.complete();
    expect(component.isMutating).toBeFalse();
  });

  it('deletes a confirmed car and handles errors', () => {
    dialog.open.and.returnValue({ afterClosed: () => of(true) } as any);
    service.deleteCar.and.returnValue(throwError(() => ({ error: { message: 'Cannot delete' } })));
    component.deleteCar(car);
    expect(component.isMutating).toBeFalse();
    expect(snack.open).toHaveBeenCalledWith('Cannot delete', 'Close', jasmine.any(Object));
  });

  it('uses compact columns on mobile', async () => {
    component.ngOnDestroy();
    (component as any).breakpointObserver = { observe: () => of({ matches: true }) };
    component.ngOnInit();
    expect(component.displayedColumns).toEqual(['car', 'pricePerDay', 'status', 'actions']);
  });
});
