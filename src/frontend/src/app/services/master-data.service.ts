import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type MasterDataTable =
  | 'machines'
  | 'standards'
  | 'instruments'
  | 'qpi';

export interface MasterDataRow {
  id: number;
  [key: string]: any;
}

@Injectable({ providedIn: 'root' })
export class MasterDataService {
  private readonly apiUrl =
    'http://localhost:5193/api/master-data';

  constructor(private readonly http: HttpClient) { }

  getRows(table: MasterDataTable): Observable<MasterDataRow[]> {
    return this.http.get<MasterDataRow[]>(
      `${this.apiUrl}/${table}`
    );
  }

  createRow(
    table: MasterDataTable,
    row: Partial<MasterDataRow>
  ): Observable<MasterDataRow> {
    return this.http.post<MasterDataRow>(
      `${this.apiUrl}/${table}`,
      row
    );
  }

  updateRow(
    table: MasterDataTable,
    id: number,
    row: Partial<MasterDataRow>
  ): Observable<MasterDataRow> {
    return this.http.put<MasterDataRow>(
      `${this.apiUrl}/${table}/${id}`,
      row
    );
  }

  deleteRow(
    table: MasterDataTable,
    id: number
  ): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${table}/${id}`
    );
  }
}
