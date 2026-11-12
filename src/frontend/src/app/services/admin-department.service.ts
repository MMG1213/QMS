
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';

export interface DepartmentOption {
  id: number;
  name: string;
}

export interface AdminDepartment {
  departmentId: number;
  departmentCode: string;
  departmentName: string;
  isActive: boolean;
}

export interface CreateDepartmentRequest {
  departmentCode: string;
  departmentName: string;
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

  // Load active departments for QMS Admin account creation.
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

  // Load all departments, including restricted departments.
  getAllDepartments(): Observable<AdminDepartment[]> {
    return this.http.get<AdminDepartment[]>(
      this.adminDepartmentsApiUrl
    );
  }

  // Create a department in QMSDepartments.
  createDepartment(
    request: CreateDepartmentRequest
  ): Observable<DepartmentStatusResponse> {
    return this.http.post<DepartmentStatusResponse>(
      this.adminDepartmentsApiUrl,
      request
    );
  }

  // Activate or restrict a department.
  updateDepartmentStatus(
    departmentId: number,
    isActive: boolean
  ): Observable<DepartmentStatusResponse> {
    return this.http.patch<DepartmentStatusResponse>(
      `${this.adminDepartmentsApiUrl}/${departmentId}/status`,
      { isActive }
    );
  }
}
