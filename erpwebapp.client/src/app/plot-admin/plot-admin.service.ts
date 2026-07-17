import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService } from '../shared/services/api.service';
import { ApiResponse, PlotAdmin, PlotAmenity, PlotVisitAdmin } from './plot-admin.models';

@Injectable({ providedIn: 'root' })
export class PlotAdminService {
  constructor(private api: ApiService) {}

  list(search = ''): Observable<PlotAdmin[]> {
    const query = search ? `?search=${encodeURIComponent(search)}` : '';
    return this.api.get<ApiResponse<PlotAdmin[]>>(`plots/admin${query}`).pipe(map(x => x.data ?? []));
  }

  create(plot: PlotAdmin): Observable<PlotAdmin> {
    return this.api.post<ApiResponse<PlotAdmin>>('plots/admin', plot).pipe(map(x => x.data));
  }

  update(plot: PlotAdmin): Observable<PlotAdmin> {
    return this.api.put<ApiResponse<PlotAdmin>>(`plots/admin/${plot.id}`, plot).pipe(map(x => x.data));
  }

  delete(id: string): Observable<void> {
    return this.api.delete<ApiResponse<null>>(`plots/admin/${id}`).pipe(map(() => undefined));
  }
  amenities(): Observable<PlotAmenity[]> {
    return this.api.get<ApiResponse<PlotAmenity[]>>('plots/admin/amenities').pipe(map(x => x.data ?? []));
  }
  visits(status = '', search = ''): Observable<PlotVisitAdmin[]> {
    const params = new URLSearchParams();
    if (status) params.set('status', status);
    if (search) params.set('search', search);
    const query = params.toString() ? `?${params}` : '';
    return this.api.get<ApiResponse<PlotVisitAdmin[]>>(`plots/admin/bookings${query}`).pipe(map(x => x.data ?? []));
  }
  updateVisitStatus(id: string, status: string): Observable<PlotVisitAdmin> {
    return this.api.put<ApiResponse<PlotVisitAdmin>>(`plots/admin/bookings/${id}/status`, { status }).pipe(map(x => x.data));
  }
}
