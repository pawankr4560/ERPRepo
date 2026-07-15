import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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

  getOrders(): Observable<ApiResponse<ConstructionOrder[]>> {
    return this.api.get<ApiResponse<ConstructionOrder[]>>(`${this.endpoint}/orders`);
  }

  getDeliveries(): Observable<ApiResponse<ConstructionDelivery[]>> {
    return this.api.get<ApiResponse<ConstructionDelivery[]>>(`${this.endpoint}/deliveries`);
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
}
