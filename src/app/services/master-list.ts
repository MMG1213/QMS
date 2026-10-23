
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentType {
  documentTypeId: number;
  typeName: string;
}

export interface Department {
  departmentId: number;
  departmentCode: string;
  departmentName: string;
}

export interface DocumentRevision {
  revisionNumber: number;
  pageCount: number;
  revisionDate: string;
}

export interface MasterListDocument {
  documentId: number;
  title: string;
  documentNo: string;
  documentTypeId: number;
  departmentId: number;
  departmentName: string;
  retentionPeriod:string;
  status:string;
  revisions: DocumentRevision[];
}

@Injectable({
  providedIn: 'root'
})
export class MasterListService {
  private readonly apiUrl = 'http://localhost:5193/api/master-list';

  constructor(private http: HttpClient) {}

  getDocumentTypes(): Observable<DocumentType[]> {
    return this.http.get<DocumentType[]>(
      `${this.apiUrl}/document-types`
    );
  }

  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(
      `${this.apiUrl}/departments`
    );
  }

  getDocuments(
    documentTypeId?: number,
    departmentId?: number,
    search?: string
  ): Observable<MasterListDocument[]> {
    let params = new HttpParams();

    if (documentTypeId !== undefined) {
      params = params.set('documentTypeId', documentTypeId);
    }

    if (departmentId !== undefined) {
      params = params.set('departmentId', departmentId);
    }

    if (search?.trim()) {
      params = params.set('search', search.trim());
    }

    return this.http.get<MasterListDocument[]>(
      `${this.apiUrl}/documents`,
      { params }
    );
  }
}
