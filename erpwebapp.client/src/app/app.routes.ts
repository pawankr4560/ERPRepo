import { Routes } from '@angular/router';
import { DynamicComponent } from './dynamic-component/dynamic-component';
import { Home } from './home/home';
import { Product } from './product/product/product';
import { Setting } from './setting/setting';
import { Dashboard } from './dashboard/dashboard';
import { User } from './users/user/user';
import { DragDrop } from './drag-drop/drag-drop';
import { Login } from './auth/login/login';
import { Component } from '@angular/core';
import { PermissionComponent } from './Menu/permissioncomponent/permissioncomponent';
import { ItemMaster } from './item/item-master/item-master';
import { Emi } from './EMI/emi/emi';
import { authGuard } from './auth/guards/auth.guard';
import { roleGuard } from './auth/guards/role.guard';
import { CarMaster } from './booking/car/car-master/car-master';
import { BookingMaster } from './booking/booking/booking-master/booking-master';


export const routes: Routes = [
   // Default redirect
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // Auth module (NO home layout)
  {
    path: 'auth',
    children: [
      { path: 'login', component: Login },
      { path: 'signup', redirectTo: '/home/users', pathMatch: 'full' },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },

  // HOME LAYOUT (PARENT OF ALL PAGES)
  {
    path: 'home',
    component: Home,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: Dashboard, canActivate: [roleGuard], data: { roles: ['admin'] } },
      { path: 'users', component: User, canActivate: [roleGuard], data: { roles: ['admin', 'user'] } },
      { path: 'settings', component: Setting, canActivate: [roleGuard], data: { roles: ['admin'] } },

      // Inventory under Home
      { path: 'booking/cars', component: CarMaster, canActivate: [roleGuard], data: { roles: ['admin'] } },
      { path: 'inventory/product', component: Product, canActivate: [roleGuard], data: { roles: ['admin'] } },
      { path: 'inventory/item', component: ItemMaster, canActivate: [roleGuard], data: { roles: ['admin'] } },
      {
        path: 'inventory/transactions',
        loadComponent: () =>
          import('./transaction/loan-component/loan-component').then(
            (component) => component.LoanComponent
          ),
        canActivate: [roleGuard],
        data: { roles: ['admin'] }
      },
      {
        path: 'inventory/transactions/:id',
        loadComponent: () =>
          import('./transaction/loan-details/loan-details').then(
            (component) => component.LoanDetailsComponent
          ),
        canActivate: [roleGuard],
        data: { roles: ['admin'] }
      },
      {
        path: 'inventory/payments',
        loadComponent: () =>
          import('./transaction/loan-payment-component/loan-payment-component').then(
            (component) => component.LoanPaymentComponent
          ),
        canActivate: [roleGuard],
        data: { roles: ['admin'] }
      },
      { path: 'inventory/emi', component: Emi, canActivate: [roleGuard], data: { roles: ['admin'] } },
     
      { path: 'booking/list', component: BookingMaster, canActivate: [roleGuard], data: { roles: ['admin'] } },
      {
        path: 'booking/payments',
        loadComponent: () =>
          import('./booking/payment/booking-payment/booking-payment').then(
            (component) => component.BookingPaymentComponent
          ),
        canActivate: [roleGuard],
        data: { roles: ['admin'] }
      },


      // Other pages
      { path: 'dynamic', component: DragDrop, canActivate: [roleGuard], data: { roles: ['admin'] } },

      //setting page
      { path: 'permission', component: PermissionComponent, canActivate: [roleGuard], data: { roles: ['admin'] } },

      // Default home route
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];
