import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, forkJoin, map, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Car } from '../../car/interfaces/car';
import { Booking, BookingUser } from '../interfaces/booking';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly bookingsSubject = new BehaviorSubject<Booking[]>([]);
  readonly bookings$ = this.bookingsSubject.asObservable();

  private get headers(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json; charset=utf-8',
      api_key: environment.apiKey,
    });
  }

  private get apiUrl(): string {
    return `${environment.apiUrl}/api/Booking`;
  }

  constructor(private http: HttpClient) {}

  loadBookings() {
    return this.http.get<Booking[]>(this.apiUrl, { headers: this.headers }).pipe(
      tap((bookings) => this.bookingsSubject.next(bookings ?? []))
    );
  }

  loadDialogOptions() {
    return forkJoin({
      cars: this.http.get<Car[]>(`${environment.apiUrl}/api/Car`, {
        headers: this.headers,
      }),
      usersResponse: this.http.get<any>(`${environment.apiUrl}/api/Auth/UserList`, {
        headers: this.headers,
      }),
    }).pipe(
      map(({ cars, usersResponse }) => ({
        cars: cars ?? [],
        users: (usersResponse?.data ?? usersResponse ?? []) as BookingUser[],
      }))
    );
  }

  createBooking(booking: Booking) {
    return this.http.post<Booking>(this.apiUrl, this.toCreatePayload(booking), {
      headers: this.headers,
    }).pipe(
      tap((created) =>
        this.bookingsSubject.next([created, ...this.bookingsSubject.value])
      )
    );
  }

  updateBooking(booking: Booking) {
    return this.http.put<Booking>(
      `${this.apiUrl}/${booking.id}`,
      {
        ...this.toCreatePayload(booking),
        status: booking.status,
        paymentStatus: booking.paymentStatus,
      },
      { headers: this.headers }
    ).pipe(
      tap((updated) =>
        this.bookingsSubject.next(
          this.bookingsSubject.value.map((current) =>
            current.id === updated.id ? updated : current
          )
        )
      )
    );
  }

  deleteBooking(id: number) {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.headers,
    }).pipe(
      tap(() =>
        this.bookingsSubject.next(
          this.bookingsSubject.value.filter((booking) => booking.id !== id)
        )
      )
    );
  }

  private toCreatePayload(booking: Booking) {
    return {
      carId: Number(booking.carId),
      userId: booking.userId,
      pickupDate: booking.pickupDate,
      returnDate: booking.returnDate,
    };
  }
}
