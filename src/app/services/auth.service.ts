
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

// ─── Login ───────────────────────────────────────────────

export interface LoginRequest {
  loginId: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  userId: number;
  employeeId: string;
  fullName: string;
  email: string;
  departmentId: number | null;
  role: string;
}

// ─── Department ─────────────────────────────────────────

export interface DepartmentOption {
  id: number;
  name: string;
}

// ─── Employee Signup ────────────────────────────────────

export type SignupRole = 'User' | 'Department Head';

export interface SignupRequest {
  employeeId: string;
  fullName: string;
  email: string;
  departmentId: number;
  role: SignupRole;
  password: string;
}

export interface SignupResponse {
  message: string;
  requestId: number;
  status: string;
}

export interface SignupRequestItem {
  id: number;
  employeeId: string;
  fullName: string;
  email: string;
  departmentId: number;
  departmentName: string | null;
  role: string;
  status: string;
  requestedAtUtc: string;
  reviewedByUserId: number | null;
  reviewedAtUtc: string | null;
}


export interface SignupReviewResponse {
  message: string;
  requestId: number;
  status: string;
}


// ─── Admin: Create Active Account ───────────────────────

export interface AdminCreateUserRequest {
  employeeId: string;
  fullName: string;
  email: string;
  password: string;
  role: SignupRole;
  departmentId: number;
}

export interface AdminCreatedUser {
  id: number;
  employeeId: string;
  fullName: string;
  email: string;
  departmentId: number | null;
  role: string;
  isActive: boolean;
}

// ─── Stored Session User ────────────────────────────────

export type CurrentUser = Omit<LoginResponse, 'token'>;

// ─── Authentication Service ─────────────────────────────

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = '/api/auth';
  private readonly signupApiUrl = '/api/signup';
  private readonly adminSignupApiUrl = '/api/admin/signup-requests';
  private readonly adminUsersApiUrl = '/api/admin/users';

  // ─── Login ──────────────────────────────────────────

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login`, request)
      .pipe(
        tap((response: LoginResponse) => {
          sessionStorage.setItem('qms.token', response.token);

          const currentUser: CurrentUser = {
            userId: response.userId,
            employeeId: response.employeeId,
            fullName: response.fullName,
            email: response.email,
            departmentId: response.departmentId,
            role: response.role,
            expiresAtUtc: response.expiresAtUtc
          };

          sessionStorage.setItem(
            'qms.user',
            JSON.stringify(currentUser)
          );
        })
      );
  }

  // ─── Current Session ─────────────────────────────────

  getCurrentUser(): CurrentUser | null {
    const raw = sessionStorage.getItem('qms.user');

    if (!raw) {
      return null;
    }

    try {
      const user = JSON.parse(raw) as CurrentUser;
      const expiryTime = new Date(user.expiresAtUtc).getTime();
      const token = sessionStorage.getItem('qms.token');

      if (
        !user.expiresAtUtc ||
        !Number.isFinite(expiryTime) ||
        expiryTime <= Date.now() ||
        !token
      ) {
        this.logout();
        return null;
      }

      return user;
    } catch {
      this.logout();
      return null;
    }
  }

  getToken(): string | null {
    if (!this.getCurrentUser()) {
      return null;
    }

    return sessionStorage.getItem('qms.token');
  }

  isLoggedIn(): boolean {
    return this.getCurrentUser() !== null;
  }

  logout(): void {
    sessionStorage.removeItem('qms.token');
    sessionStorage.removeItem('qms.user');
  }

  // ─── Public Signup Departments ───────────────────────

  getActiveDepartments(): Observable<DepartmentOption[]> {
    return this.http.get<DepartmentOption[]>(
      '/api/departments/active'
    );
  }

  // ─── Submit Signup Request ───────────────────────────

  submitSignup(
    request: SignupRequest
  ): Observable<SignupResponse> {
    return this.http.post<SignupResponse>(
      this.signupApiUrl,
      request
    );
  }

  // ─── Admin: Signup Requests ──────────────────────────

  getSignupRequests(
    status = 'Pending'
  ): Observable<SignupRequestItem[]> {
    const encodedStatus = encodeURIComponent(status);

    return this.http.get<SignupRequestItem[]>(
      `${this.adminSignupApiUrl}?status=${encodedStatus}`
    );
  }

  reviewSignupRequest(
    id: number,
    decision: 'Approve' | 'Reject'
  ): Observable<SignupReviewResponse> {
    return this.http.post<SignupReviewResponse>(
      `${this.adminSignupApiUrl}/${id}/review`,
      { decision }
    );
  }

  getAdminDepartments(): Observable<DepartmentOption[]> {
  return this.http.get<DepartmentOption[]>(
    `${this.adminUsersApiUrl}/departments`
  );
}

createAdminUser(
  request: AdminCreateUserRequest
): Observable<AdminCreatedUser> {
  return this.http.post<AdminCreatedUser>(
    this.adminUsersApiUrl,
    request
  );
}
}
