import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ReimbursementRequest, ReimbursementStatus } from '../models/reimbursement-request.model';

/// Único lugar del panel que conoce las rutas de la API. Ningún componente inyecta HttpClient.
@Injectable({ providedIn: 'root' })
export class ReimbursementApiService {
  private readonly http = inject(HttpClient);

  private readonly baseUrl = 'http://localhost:5080/api/v1/reimbursement-requests';

  getRequests(status: ReimbursementStatus | null): Observable<ReimbursementRequest[]> {
    // Sin filtro no se envía el parámetro: el backend devuelve todo cuando status es null.
    const params = status ? new HttpParams().set('status', status) : new HttpParams();

    return this.http.get<ReimbursementRequest[]>(this.baseUrl, { params });
  }

  approve(id: string): Observable<ReimbursementRequest> {
    return this.http.post<ReimbursementRequest>(`${this.baseUrl}/${id}/approve`, {});
  }

  reject(id: string, reason: string): Observable<ReimbursementRequest> {
    return this.http.post<ReimbursementRequest>(`${this.baseUrl}/${id}/reject`, { reason });
  }
}
