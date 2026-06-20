import { Car } from '../../car/interfaces/car';

export interface BookingUser {
  id: string;
  firstName: string;
  lastName: string;
  email?: string | null;
}

export interface Booking {
  id: number;
  bookingNumber: string;
  userId: string;
  userName: string;
  carId: number;
  carName: string;
  pickupDate: string;
  returnDate: string;
  totalDays: number;
  amount: number;
  status: string;
  paymentStatus: string;
  createdDate: string;
}

export type BookingDialogMode = 'create' | 'edit' | 'view';

export interface BookingDialogData {
  mode: BookingDialogMode;
  booking?: Booking;
  cars: Car[];
  users: BookingUser[];
}
