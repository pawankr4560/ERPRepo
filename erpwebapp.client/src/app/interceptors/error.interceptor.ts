import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, throwError } from 'rxjs';
import { ToastService } from '../shared/services/toast.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(
    private router: Router,
    private toastService: ToastService
  ) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    if (request.headers.has('X-Skip-Error-Toast')) {
      return next.handle(request).pipe(catchError((error: HttpErrorResponse) => throwError(() => error)));
    }

    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        this.handleError(error);
        return throwError(() => error);
      })
    );
  }

  private handleError(error: HttpErrorResponse): void {
    switch (error.status) {
      case 400:
        this.toastService.show(this.getBadRequestMessage(error), 'error');
        break;
      case 401:
        localStorage.clear();
        this.toastService.show('Your session has expired. Please log in again.', 'warning');
        this.router.navigate(['/auth/login']);
        break;
      case 403:
        this.toastService.show('Access denied', 'error');
        break;
      case 404:
        // Feature pages provide a more useful, contextual not-found message.
        break;
      case 500:
        // Avoid duplicating the page-level error notification.
        break;
      default:
        // Network and feature errors are handled by the requesting screen.
        break;
    }
  }

  private getBadRequestMessage(error: HttpErrorResponse): string {
    const validationMessage = this.firstValidationError(error.error?.errors);
    return (
      error.error?.errorMessage ??
      error.error?.message ??
      validationMessage ??
      'Bad request. Please check the entered details.'
    );
  }

  private firstValidationError(errors: Record<string, string[]> | undefined): string | null {
    if (!errors) {
      return null;
    }

    return Object.values(errors).flat()[0] ?? null;
  }
}

