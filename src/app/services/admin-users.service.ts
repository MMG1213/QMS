
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  AdminCreateUserRequest,
  AdminCreatedUser
} from './signin.service';

@Injectable({
  providedIn: 'root'
})
export class AdminUsersService {
  private readonly http = inject(HttpClient);

  private readonly adminUsersApiUrl = '/api/admin/users';

  // Create an active user account as a QMS Admin.
  createAdminUser(
    request: AdminCreateUserRequest
  ): Observable<AdminCreatedUser> {
    return this.http.post<AdminCreatedUser>(
      this.adminUsersApiUrl,
      request
    );
  }
}
