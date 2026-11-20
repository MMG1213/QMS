
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// Request sent to POST /api/DCR
export interface DCRCreateRequest {
  requestDate: string;
  standard: string;
  requestType: string;
  documentTypeName: string;
  documentId?: number | null;
  documentTitle?: string | null;
  documentNo?: string | null;
  descriptionBeforechange?: string | null;
  descriptionAfterchange?: string | null;
  reasonforchange?: string | null;
}

// Response type can be refined after checking
// the actual response returned by DCRController.
export interface DCRCreateResponse {
  message?: string;
  dcrRequestId?: number;
  dcrReferenceNumber?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DcrService {

  private readonly apiUrl = 'http://localhost:5193/api';

  constructor(private http: HttpClient) {}

  // Create a new Document Change Request
  createDCR(
    request: DCRCreateRequest
  ): Observable<DCRCreateResponse> {

    return this.http.post<DCRCreateResponse>(
      `${this.apiUrl}/DCR`,
      request
    );
  }

  // Get pending DCRs assigned to the logged-in approver
  getPendingDCRs(): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.apiUrl}/DCRPending`
    );
  }

}
