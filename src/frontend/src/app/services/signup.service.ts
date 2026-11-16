
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  SignupRequest,
  SignupResponse,
  SignupRequestItem,
  SignupReviewResponse
} from './signin.service';

@Injectable({
  providedIn: 'root'
})
export class SignupService {
  private readonly http = inject(HttpClient);

  private readonly signupApiUrl = '/api/signup';
  private readonly adminSignupApiUrl = '/api/admin/signup-requests';

  // Submit a new employee signup request.
  submitSignup(
    request: SignupRequest
  ): Observable<SignupResponse> {
    return this.http.post<SignupResponse>(
      this.signupApiUrl,
      request
    );
  }

  // Retrieve signup requests by status.
  getSignupRequests(
    status = 'Pending'
  ): Observable<SignupRequestItem[]> {
    const encodedStatus = encodeURIComponent(status);

    return this.http.get<SignupRequestItem[]>(
      `${this.adminSignupApiUrl}?status=${encodedStatus}`
    );
  }

  // Approve or reject an employee signup request.
  reviewSignupRequest(
    id: number,
    decision: 'Approve' | 'Reject'
  ): Observable<SignupReviewResponse> {
    return this.http.post<SignupReviewResponse>(
      `${this.adminSignupApiUrl}/${id}/review`,
      { decision }
    );
  }
}
