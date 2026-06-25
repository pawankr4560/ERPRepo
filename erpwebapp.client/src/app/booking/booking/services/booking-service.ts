import { Injectable } from '@angular/core';
import { BehaviorSubject, map, tap } from 'rxjs';

import { ApiService } from '../../../shared/services/api.service';
import { Car } from '../../car/interfaces/car';
import { Booking, BookingUser } from '../interfaces/booking';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly bookingsSubject = new BehaviorSubject<Booking[]>([]);
  readonly bookings$ = this.bookingsSubject.asObservable();

  constructor(private api: ApiService) {}

  loadBookings() {
    return this.api.get<any>('Booking').pipe(
      map((response) =>
        this.unwrapArray(response).map((booking) => this.normalizeBooking(booking))
      ),
      tap((bookings) => this.bookingsSubject.next(bookings))
    );
  }

  loadDialogOptions() {
    return this.api
      .get<any>('Booking/options')
      .pipe(
        map((response) => {
          const data = response?.data ?? response?.Data ?? response ?? {};
          return {
            cars: this.unwrapArray(data.cars ?? data.Cars).map((car) =>
              this.normalizeCar(car)
            ),
            users: this.unwrapArray(data.users ?? data.Users).map((user) =>
              this.normalizeUser(user)
            ),
          };
        })
      );
  }

  createBooking(booking: Booking) {
    return this.api.post<any>('Booking', this.toCreatePayload(booking)).pipe(
      map((response) => this.normalizeBooking(response?.data ?? response)),
      tap((created) =>
        this.bookingsSubject.next([created, ...this.bookingsSubject.value])
      )
    );
  }

  updateBooking(booking: Booking) {
    return this.api.put<any>(
      `Booking/${booking.id}`,
      {
        ...this.toCreatePayload(booking),
        status: booking.status,
      }
    ).pipe(
      map((response) => this.normalizeBooking(response?.data ?? response)),
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
    return this.api.delete<void>(`Booking/${id}`).pipe(
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

  private unwrapArray(value: any): any[] {
    const data = value?.data ?? value?.Data ?? value;
    return Array.isArray(data) ? data : [];
  }

  private normalizeBooking(value: any): Booking {
    return {
      id: value?.id ?? value?.Id ?? 0,
      bookingNumber: value?.bookingNumber ?? value?.BookingNumber ?? '',
      userId: value?.userId ?? value?.UserId ?? '',
      userName: value?.userName ?? value?.UserName ?? '',
      carId: value?.carId ?? value?.CarId ?? 0,
      carName: value?.carName ?? value?.CarName ?? '',
      pickupDate: value?.pickupDate ?? value?.PickupDate ?? '',
      returnDate: value?.returnDate ?? value?.ReturnDate ?? '',
      totalDays: value?.totalDays ?? value?.TotalDays ?? 0,
      amount: value?.amount ?? value?.Amount ?? 0,
      status: value?.status ?? value?.Status ?? 'Pending',
      paymentStatus:
        value?.paymentStatus ?? value?.PaymentStatus ?? 'Pending',
      createdDate: value?.createdDate ?? value?.CreatedDate ?? '',
    };
  }

  private normalizeCar(value: any): Car {
    return {
      id: value?.id ?? value?.Id ?? 0,
      brand: value?.brand ?? value?.Brand ?? '',
      model: value?.model ?? value?.Model ?? '',
      year: value?.year ?? value?.Year ?? 0,
      categoryId: value?.categoryId ?? value?.CategoryId ?? 0,
      transmission: value?.transmission ?? value?.Transmission ?? '',
      fuelType: value?.fuelType ?? value?.FuelType ?? '',
      seats: value?.seats ?? value?.Seats ?? 0,
      pricePerDay: value?.pricePerDay ?? value?.PricePerDay ?? 0,
      imageUrl: value?.imageUrl ?? value?.ImageUrl ?? null,
      status: value?.status ?? value?.Status ?? 'Available',
    };
  }

  private normalizeUser(value: any): BookingUser {
    return {
      id: value?.id ?? value?.Id ?? '',
      firstName: value?.firstName ?? value?.FirstName ?? '',
      lastName: value?.lastName ?? value?.LastName ?? '',
      email: value?.email ?? value?.Email ?? null,
    };
  }
}
