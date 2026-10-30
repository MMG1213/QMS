
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface DepartmentOption {
  id: number;
  name: string;
}

export interface AdminDepartment {
  id: number;
  deptName: string;
  deptId: string;
  isActive: boolean;
}

export interface CreateDepartmentRequest {
  deptName: string;
  deptId: string;
}

export interface DepartmentStatusResponse {
  message: string;
  department: AdminDepartment;
}

interface DepartmentApiOption {
  id: number;
  deptName: string;
}

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private readonly http = inject(HttpClient);

  private readonly departmentsApiUrl = '/api/departments';
  private readonly adminDepartmentsApiUrl = '/api/admin/departments';
  private readonly adminUsersApiUrl = '/api/admin/users';

  // Load active departments for signup forms.
  getActiveDepartments(): Observable<DepartmentOption[]> {
    return this.http
      .get<DepartmentApiOption[]>(
        `${this.departmentsApiUrl}/active`
      )
      .pipe(
        map(departments =>
          departments.map(department => ({
            id: department.id,
            name: department.deptName
          }))
        )
      );
  }

  // Load departments for the existing admin account-creation form.
  getAdminDepartments(): Observable<DepartmentOption[]> {
    return this.http
      .get<DepartmentApiOption[]>(
        `${this.adminUsersApiUrl}/departments`
      )
      .pipe(
        map(departments =>
          departments.map(department => ({
            id: department.id,
            name: department.deptName
          }))
        )
      );
  }

  // Load all departments, including inactive departments.
  getAllDepartments(): Observable<AdminDepartment[]> {
    return this.http.get<AdminDepartment[]>(
      this.adminDepartmentsApiUrl
    );
  }

  // Create a new department.
  createDepartment(
    request: CreateDepartmentRequest
  ): Observable<DepartmentStatusResponse> {
    return this.http.post<DepartmentStatusResponse>(
      this.adminDepartmentsApiUrl,
      request
    );
  }

  // Activate or deactivate a department.
  updateDepartmentStatus(
    id: number,
    isActive: boolean
  ): Observable<DepartmentStatusResponse> {
    return this.http.patch<DepartmentStatusResponse>(
      `${this.adminDepartmentsApiUrl}/${id}/status`,
      { isActive }
    );
  }
}
