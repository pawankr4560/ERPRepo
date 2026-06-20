import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { Auth } from '../auth';
import { MatSnackBar } from '@angular/material/snack-bar';

export const roleGuard: CanActivateFn = (route, state) => {
  const auth = inject(Auth);
  const router = inject(Router);
  const snackBar = inject(MatSnackBar);

  const userRole = auth.getRole();
  const expectedRoles = route.data?.['roles'] as Array<string>;

  if (userRole && expectedRoles && expectedRoles.map(r => r.toLowerCase()).includes(userRole.toLowerCase())) {
    return true;
  }

  snackBar.open('Unauthorized access ❌', 'Close', {
    duration: 3000,
    horizontalPosition: 'right',
    verticalPosition: 'top',
    panelClass: ['error-snackbar']
  });

  if (userRole && userRole.toLowerCase() === 'admin') {
    router.navigate(['/home/dashboard']);
  } else {
    router.navigate(['/home/users']);
  }
  return false;
};
