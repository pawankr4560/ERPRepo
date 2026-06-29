import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { Auth } from '../auth';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(Auth);
  const token = localStorage.getItem('auth_token') ?? localStorage.getItem('jwt');
  if (token) {
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      if (decoded.exp && decoded.exp < currentTime) {
        authService.clearSession();
        router.navigate(['/auth/login']);
        return false;
      }
      return true;
    } catch (e) {
      authService.clearSession();
      router.navigate(['/auth/login']);
      return false;
    }
  }
  router.navigate(['/auth/login']);
  return false;
};
