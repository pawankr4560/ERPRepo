import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { AuthGuard } from './auth.guard';

@Injectable({
  providedIn: 'root',
})
export class AdminGuard extends AuthGuard implements CanActivate {
  constructor(router: Router) {
    super(router);
  }

  override canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    if (!super.canActivate(route, state)) {
      return false;
    }

    const payload = this.getTokenPayload();
    const role =
      payload?.['role'] ??
      payload?.['Role'] ??
      payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    if (String(role).toLowerCase() === 'admin') {
      return true;
    }

    this.router.navigate(['/home/users']);
    return false;
  }
}
