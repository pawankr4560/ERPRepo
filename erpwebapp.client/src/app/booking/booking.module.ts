import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { BookingMaster } from './booking/booking-master/booking-master';
import { CarMaster } from './car/car-master/car-master';
import { BookingPaymentComponent } from './payment/booking-payment/booking-payment';
import { AuthGuard } from '../guards/auth.guard';
import { AdminGuard } from '../guards/admin.guard';

const routes: Routes = [
  { path: 'cars', component: CarMaster, canActivate: [AdminGuard] },
  {
    path: 'items',
    loadChildren: () =>
      import('../item/item.module').then((module) => module.ItemModule),
    canActivate: [AdminGuard],
  },
  { path: 'list', component: BookingMaster, canActivate: [AuthGuard] },
  { path: 'payments', component: BookingPaymentComponent, canActivate: [AdminGuard] },
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
