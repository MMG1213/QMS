
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface DocumentType {
  typeCode: string;
  documentTypeId: number;
  typeName: string;
}

export interface Department {
  departmentId: number;
  departmentCode: string;
  departmentName: string;
}

export interface DocumentRevision {
  id: number;
  revisionNumber: number;
  pageCount: number;
  revisionDate: string;
}

export interface MasterListDocument {
typeCode: any;
  documentId: number;
  title: string;
  documentNo: string;
  documentTypeId: number;
  departmentId: number;
  departmentName: string;
  retentionPeriod: string | null;
  status: string;
  obseleteDate: string | null;
  revisions: DocumentRevision[];
  typeName: string | null;
}

export interface UpdateMasterListDocument {
  documentNo: string;
  title: string;
  documentTypeId: number;
  departmentId: number;
  retentionPeriod: string | null;
  status: string;
  obseleteDate: string | null;
  revisions: {
    revisionNumber: number;
    pageCount: number;
    revisionDate: string;
  }[];
}
export interface ML01Revision {
  id: number;
  revisionNo: number;
  pageCount: number;
  revisionDate: string;
}

export interface ML01Entry {
  id: number;
  description: string;
  documentNo: string;
  revisions: ML01Revision[];
}

export interface ML01RevisionRequest {
  revisionNo: number;
  pageCount: number;
  revisionDate: string;
}

export interface ML01EntryRequest {
  description: string;
  documentNo: string;
  revisions: ML01RevisionRequest[];
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

  updateDocument(
    documentId: number,
    request: UpdateMasterListDocument
  ): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.apiUrl}/documents/${documentId}`,
      request
    );
  }

  // --------------------------------------------------
// ML01 - MASTER LIST OF MASTER LISTS
// --------------------------------------------------

private readonly ml01ApiUrl = 'http://localhost:5193/api/ML01';

getML01Entries(): Observable<ML01Entry[]> {
  return this.http.get<ML01Entry[]>(this.ml01ApiUrl);
}

createML01Entry(
  request: ML01EntryRequest
): Observable<ML01Entry> {
  return this.http.post<ML01Entry>(
    this.ml01ApiUrl,
    request
  );
}

updateML01Entry(
  id: number,
  request: ML01EntryRequest
): Observable<ML01Entry> {
  return this.http.put<ML01Entry>(
    `${this.ml01ApiUrl}/${id}`,
    request
  );
}
}
