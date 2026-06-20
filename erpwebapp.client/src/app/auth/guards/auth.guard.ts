import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('jwt');
  if (token) {
    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Date.now() / 1000;
      if (decoded.exp && decoded.exp < currentTime) {
        localStorage.removeItem('jwt');
        router.navigate(['/auth/login']);
        return false;
      }
      return true;
    } catch (e) {
      localStorage.removeItem('jwt');
      router.navigate(['/auth/login']);
      return false;
    }
  }
  router.navigate(['/auth/login']);
  return false;
};
