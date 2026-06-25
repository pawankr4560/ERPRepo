import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { BookingMaster } from './booking/booking-master/booking-master';
import { CarMaster } from './car/car-master/car-master';
import { BookingPaymentComponent } from './payment/booking-payment/booking-payment';

const routes: Routes = [
  { path: 'cars', component: CarMaster },
  { path: 'list', component: BookingMaster },
  { path: 'payments', component: BookingPaymentComponent },
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
    CarMaster,
    BookingMaster,
    BookingPaymentComponent,
  ],
})
export class BookingModule {}
