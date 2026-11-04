import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  AdminCreateUserRequest,
  AdminCreatedUser
} from './signin.service';

export interface AdminUserAccount {
  id: number;
  employeeId: string;
  fullName: string;
  email: string;
  departmentId: number | null;
  departmentName: string | null;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
}

export interface AdminUserStatusRequest {
  isActive: boolean;
}

export interface AdminUserStatusResponse {
  message: string;
  id: number;
  isActive: boolean;
}

export interface AuthorizationTestResult {
  label: string;
  endpoint: string;
  expected: string;
  actual: string;
  passed: boolean;
  message: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminUsersService {
  private readonly http = inject(HttpClient);

  private readonly adminUsersApiUrl = '/api/admin/users';

  private readonly adminSignupApiUrl =
    '/api/admin/signup-requests';

  // GET: Retrieve all user accounts.
  getAdminUsers(): Observable<AdminUserAccount[]> {
    return this.http.get<AdminUserAccount[]>(
      this.adminUsersApiUrl
    );
  }

  // PATCH: Activate or deactivate an account.
  updateUserStatus(
    id: number,
    isActive: boolean
  ): Observable<AdminUserStatusResponse> {
    return this.http.patch<AdminUserStatusResponse>(
      `${this.adminUsersApiUrl}/${id}/status`,
      { isActive } satisfies AdminUserStatusRequest
    );
  }

  // POST: Create a new active account.
  createAdminUser(
    request: AdminCreateUserRequest
  ): Observable<AdminCreatedUser> {
    return this.http.post<AdminCreatedUser>(
      this.adminUsersApiUrl,
      request
    );
  }

  // Authorization testing: QMS Admin-only endpoint.
  testAdminUsersEndpoint(): Observable<unknown> {
    return this.http.get<unknown>(
      this.adminUsersApiUrl
    );
  }

  // Authorization testing: QMS Admin-only endpoint.
  testAdminSignupRequestsEndpoint(): Observable<unknown> {
    return this.http.get<unknown>(
      `${this.adminSignupApiUrl}?status=Pending`
    );
  }
}