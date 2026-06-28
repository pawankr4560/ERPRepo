import { Routes } from '@angular/router';
import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
   // Default redirect
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // Auth module (NO home layout)
  {
    path: 'auth',
    loadChildren: () =>
      import('./auth/auth.module').then((module) => module.AuthModule),
  },

  // HOME LAYOUT (PARENT OF ALL PAGES)
  {
    path: 'home',
    loadComponent: () =>
      import('./home/home').then((component) => component.Home),
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./dashboard/dashboard.module').then(
            (module) => module.DashboardModule
          ),
        canActivate: [AdminGuard],
      },
      {
        path: 'users',
        loadChildren: () =>
          import('./users/users.module').then((module) => module.UsersModule),
        canActivate: [AuthGuard],
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./setting/setting.module').then(
            (module) => module.SettingModule
          ),
        canActivate: [AdminGuard],
      },

      // Inventory under Home
      {
        path: 'booking',
        loadChildren: () =>
          import('./booking/booking.module').then(
            (module) => module.BookingModule
          ),
        canActivate: [AuthGuard],
      },
      {
        path: 'inventory',
        children: [
          {
            path: 'product',
            loadChildren: () =>
              import('./product/product.module').then(
                (module) => module.ProductModule
              ),
            canActivate: [AdminGuard],
          },
          {
            path: 'item',
            loadChildren: () =>
              import('./item/item.module').then((module) => module.ItemModule),
            canActivate: [AuthGuard],
          },
          {
            path: 'transactions',
            loadChildren: () =>
              import('./transaction/loan.module').then(
                (module) => module.LoanModule
              ),
            canActivate: [AdminGuard],
          },
          {
            path: 'payments',
            loadChildren: () =>
              import('./transaction/loan-payment.module').then(
                (module) => module.LoanPaymentModule
              ),
            canActivate: [AdminGuard],
          },
          {
            path: 'emi',
            loadChildren: () =>
              import('./EMI/emi.module').then((module) => module.EmiModule),
            canActivate: [AdminGuard],
          },
        ],
      },
      {
        path: 'pay-emi',
        loadChildren: () =>
          import('./transaction/emi-pay.module').then(
            (module) => module.EmiPayModule
          ),
        canActivate: [AuthGuard],
      },
      {
        path: 'sales-order',
        loadChildren: () =>
          import('./sales-order/sales-order.module').then(
            (module) => module.SalesOrderModule
          ),
        canActivate: [AuthGuard],
      },
     
      // Other pages
      {
        path: 'dynamic',
        loadComponent: () =>
          import('./drag-drop/drag-drop').then(
            (component) => component.DragDrop
          ),
        canActivate: [AuthGuard],
      },

      //setting page
      {
        path: 'permission',
        loadChildren: () =>
          import('./Menu/permission.module').then(
            (module) => module.PermissionModule
          ),
        canActivate: [AuthGuard],
      },
      {
        path: 'menu',
        loadChildren: () =>
          import('./Menu/menu.module').then((module) => module.MenuModule),
        canActivate: [AuthGuard],
      },

      // Default home route
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];
