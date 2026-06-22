import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { CarService } from './car-service';

describe('CarService', () => {
  let service: CarService;
  let http: HttpTestingController;
  const car = { id: 1, brand: 'Honda', model: 'City', year: 2025, categoryId: 2, transmission: 'Automatic', fuelType: 'Petrol', seats: 5, pricePerDay: 2500, status: 'Available' };

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [CarService, provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(CarService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it('loads cars and updates the observable store', () => {
    let stored: any[] = [];
    service.cars$.subscribe((cars) => (stored = cars));
    service.loadCars().subscribe();
    const request = http.expectOne(`${environment.apiUrl}/api/Car`);
    expect(request.request.headers.get('api_key')).toBe(environment.apiKey);
    request.flush([car]);
    expect(stored).toEqual([car]);
  });

  it('loads car categories', () => {
    service.loadCategories().subscribe((items) => expect(items[0].name).toBe('SUV'));
    http.expectOne(`${environment.apiUrl}/api/car-categories`).flush([{ id: 1, name: 'SUV' }]);
  });

  it('creates, updates, and deletes cars in the store', () => {
    service.createCar(car).subscribe();
    http.expectOne(`${environment.apiUrl}/api/Car`).flush(car);
    service.updateCar({ ...car, brand: 'Updated' }).subscribe();
    const update = http.expectOne(`${environment.apiUrl}/api/Car/1`);
    expect(update.request.method).toBe('PUT');
    expect(update.request.body.id).toBeUndefined();
    update.flush({ ...car, brand: 'Updated' });
    service.deleteCar(1).subscribe();
    http.expectOne(`${environment.apiUrl}/api/Car/1`).flush(null);
    let stored: any[] = [];
    service.cars$.subscribe((cars) => (stored = cars));
    expect(stored).toEqual([]);
  });
});
