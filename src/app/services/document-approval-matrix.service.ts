import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentApprovalMatrix {
  id?: number;
  documentTypeId: number;
  approvedBy: string | null;
  checkedBy: string | null;
  issuedBy: string | null;
  approvedDate: string | null;
  checkedDate: string | null;
  issuedDate: string | null;
}

export interface SaveDocumentApprovalMatrixRequest {
  documentTypeId: number;
  approvedBy: string | null;
  checkedBy: string | null;
  issuedBy: string | null;
  approvedDate: string | null;
  checkedDate: string | null;
  issuedDate: string | null;
}

@Injectable({ providedIn: 'root' })
export class DocumentApprovalMatrixService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5193/api/document-approval-matrix';

  getMatrix(documentTypeId: number): Observable<DocumentApprovalMatrix> {
    return this.http.get<DocumentApprovalMatrix>(`${this.apiUrl}/${documentTypeId}`);
  }

  saveMatrix(request: SaveDocumentApprovalMatrixRequest): Observable<unknown> {
    return this.http.put(this.apiUrl, request);
  }
}
