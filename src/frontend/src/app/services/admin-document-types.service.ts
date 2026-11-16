import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AdminDocumentType {
  documentTypeId: number;
  typeCode: string;
  typeName: string;
  isActive: boolean;
}

export interface CreateDocumentTypeRequest {
  typeCode: string;
  typeName: string;
}

export interface DocumentTypeOperationResponse {
  message: string;
  documentType: AdminDocumentType;
}

@Injectable({
  providedIn: 'root'
})
export class AdminDocumentTypesService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/admin/document-types';

  getAll(): Observable<AdminDocumentType[]> {
    return this.http.get<AdminDocumentType[]>(
      this.apiUrl
    );
  }

  create(
    request: CreateDocumentTypeRequest
  ): Observable<DocumentTypeOperationResponse> {
    return this.http.post<DocumentTypeOperationResponse>(
      this.apiUrl,
      request
    );
  }

  updateStatus(
    id: number,
    isActive: boolean
  ): Observable<DocumentTypeOperationResponse> {
    return this.http.patch<DocumentTypeOperationResponse>(
      `${this.apiUrl}/${id}/status`,
      { isActive }
    );
  }
}