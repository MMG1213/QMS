import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MasterListExportService {

  private readonly apiUrl = 'http://localhost:5193/api/master-list/export';

  constructor(private http: HttpClient) {}

  exportMasterList(
    documentTypeId?: number | null,
    departmentId?: number | null
  ): Observable<Blob> {

    let params = new HttpParams();

    if (documentTypeId != null) {
      params = params.set('documentTypeId', documentTypeId.toString());
    }

    if (departmentId != null) {
      params = params.set('departmentId', departmentId.toString());
    }

    return this.http.get(this.apiUrl, {
      params,
      responseType: 'blob'
    });
  }
}