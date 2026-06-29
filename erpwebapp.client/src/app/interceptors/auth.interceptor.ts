import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, finalize, switchMap, throwError } from 'rxjs';
import { LoadingService } from '../shared/services/loading.service';
import { Auth } from '../auth/auth';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private loadingService: LoadingService,
    private authService: Auth
  ) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const token = localStorage.getItem('auth_token');
    const authRequest = token
      ? request.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`,
          },
        })
      : request;

    this.loadingService.show();

    return next.handle(authRequest).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !this.isAuthEndpoint(request.url)) {
          const accessToken = localStorage.getItem('auth_token') ?? localStorage.getItem('jwt') ?? '';
          const refreshToken = localStorage.getItem('refresh_token') ?? '';

          if (accessToken && refreshToken) {
            return this.authService.refreshToken({ accessToken, refreshToken }).pipe(
              switchMap((response) => {
                if (!response.success || !response.data?.accessToken) {
                  this.endSession();
                  return throwError(() => error);
                }

                this.authService.storeTokens(response.data);
                const retryRequest = request.clone({
                  setHeaders: {
                    Authorization: `Bearer ${response.data.accessToken}`,
                  },
                });
                return next.handle(retryRequest);
              }),
              catchError((refreshError) => {
                this.endSession();
                return throwError(() => refreshError);
              })
            );
          }

          this.endSession();
        }

        return throwError(() => error);
      }),
      finalize(() => this.loadingService.hide())
    );
  }

  private isAuthEndpoint(url: string): boolean {
    return /\/Auth\/(Login|RefreshToken|GoogleLogin|Signup|ConfirmEmail)/i.test(url);
  }

  private endSession(): void {
    this.authService.clearSession();
    this.router.navigate(['/auth/login']);
  }
}
