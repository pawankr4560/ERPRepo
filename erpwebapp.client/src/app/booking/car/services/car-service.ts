import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Car, CarCategory } from '../interfaces/car';

@Injectable({ providedIn: 'root' })
export class CarService {
  private readonly carsSubject = new BehaviorSubject<Car[]>([]);
  readonly cars$ = this.carsSubject.asObservable();

  private get apiUrl(): string {
    return `${environment.apiUrl}/api/Car`;
  }

  private get headers(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
      api_key: environment.apiKey,
    });
  }

  constructor(private http: HttpClient) {}

  loadCars() {
    return this.http.get<Car[]>(this.apiUrl, { headers: this.headers }).pipe(
      tap((cars) => this.carsSubject.next(cars ?? []))
    );
  }

  loadCategories() {
    return this.http.get<CarCategory[]>(
      `${environment.apiUrl}/api/car-categories`,
      { headers: this.headers }
    );
  }

  createCar(car: Car) {
    return this.http.post<Car>(this.apiUrl, this.toPayload(car), {
      headers: this.headers,
    }).pipe(
      tap((created) => {
        this.carsSubject.next([...this.carsSubject.value, created]);
      })
    );
  }

  updateCar(car: Car) {
    return this.http.put<Car>(`${this.apiUrl}/${car.id}`, this.toPayload(car), {
      headers: this.headers,
    }).pipe(
      tap((updated) => {
        this.carsSubject.next(
          this.carsSubject.value.map((current) =>
            current.id === updated.id ? updated : current
          )
        );
      })
    );
  }

  deleteCar(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.headers,
    }).pipe(
      tap(() => {
        this.carsSubject.next(
          this.carsSubject.value.filter((car) => car.id !== id)
        );
      })
    );
  }

  private toPayload(car: Car) {
    return {
      brand: car.brand,
      model: car.model,
      year: Number(car.year),
      categoryId: Number(car.categoryId),
      transmission: car.transmission,
      fuelType: car.fuelType,
      seats: Number(car.seats),
      pricePerDay: Number(car.pricePerDay),
      imageUrl: car.imageUrl || null,
      status: car.status,
    };
  }
}
