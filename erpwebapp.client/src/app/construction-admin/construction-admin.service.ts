import { HttpContext } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SKIP_GLOBAL_LOADING } from '../shared/http/loading-context';
import { ApiService } from '../shared/services/api.service';
import {
  ApiResponse,
  ConstructionDelivery,
  ConstructionOrder,
} from './construction-admin.models';

@Injectable({ providedIn: 'root' })
export class ConstructionAdminService {
  private readonly endpoint = 'construction';

  constructor(private api: ApiService) {}

  getOrders(silent = false): Observable<ApiResponse<ConstructionOrder[]>> {
    return this.api.get<ApiResponse<ConstructionOrder[]>>(
      `${this.endpoint}/orders`,
      this.loadingContext(silent)
    );
  }

  getDeliveries(silent = false): Observable<ApiResponse<ConstructionDelivery[]>> {
    return this.api.get<ApiResponse<ConstructionDelivery[]>>(
      `${this.endpoint}/deliveries`,
      this.loadingContext(silent)
    );
  }

  createDeliveryForOrder(orderId: number): Observable<ApiResponse<unknown>> {
    return this.api.post<ApiResponse<unknown>>(
      `${this.endpoint}/deliveries/from-order/${orderId}`,
      {}
    );
  }

  updateOrderStatus(orderId: number, status: number): Observable<ApiResponse<unknown>> {
    return this.api.put<ApiResponse<unknown>>(
      `${this.endpoint}/orders/${orderId}/status`,
      { status }
    );
  }

  updateDelivery(
    deliveryId: number,
    request: {
      vehicleNumber: string | null;
      driverName: string | null;
      estimatedArrivalTime: string | null;
      status: number;
    }
  ): Observable<ApiResponse<unknown>> {
    return this.api.put<ApiResponse<unknown>>(
      `${this.endpoint}/deliveries/${deliveryId}`,
      request
    );
  }

  private loadingContext(silent: boolean): HttpContext | undefined {
    return silent
      ? new HttpContext().set(SKIP_GLOBAL_LOADING, true)
      : undefined;
  }
}
