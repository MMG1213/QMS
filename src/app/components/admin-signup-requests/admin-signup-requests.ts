
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject
} from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import {
  AdminCreateUserRequest,
  AdminCreatedUser,
  AuthService,
  DepartmentOption,
  SignupRequestItem
} from '../../services/auth.service';

type AccountRole = 'User' | 'Department Head';

@Component({
  selector: 'app-admin-signup-requests',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-signup-requests.html',
  styleUrl: './admin-signup-requests.css'
})
export class AdminSignupRequestsComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  requests: SignupRequestItem[] = [];
  adminDepartments: DepartmentOption[] = [];

  readonly accountRoles: AccountRole[] = [
    'User',
    'Department Head'
  ];

  selectedStatus = 'Pending';

  isLoading = false;
  isLoadingDepartments = false;
  isCreatingAccount = false;
  reviewingId: number | null = null;

  errorMessage = '';
  successMessage = '';
  createAccountError = '';
  createAccountSuccess = '';

  private readonly passwordPattern =
    /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,128}$/;

  createAccountForm = this.fb.nonNullable.group({
    employeeId: [
      '',
      [
        Validators.required,
        Validators.maxLength(50)
      ]
    ],
    fullName: [
      '',
      [
        Validators.required,
        Validators.maxLength(150)
      ]
    ],
    email: [
      '',
      [
        Validators.required,
        Validators.email,
        Validators.maxLength(256)
      ]
    ],
    departmentId: [
      0,
      [
        Validators.required,
        Validators.min(1)
      ]
    ],
    role: [
      'User' as AccountRole,
      Validators.required
    ],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(128),
        Validators.pattern(this.passwordPattern)
      ]
    ]
  });

  ngOnInit(): void {
    this.loadRequests();
    this.loadAdminDepartments();
  }

  loadRequests(): void {
    if (this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService
      .getSignupRequests(this.selectedStatus)
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response: SignupRequestItem[]) => {
          if (!Array.isArray(response)) {
            this.requests = [];
            this.errorMessage =
              'The server returned an unexpected response format.';
            return;
          }

          this.requests = response;
        },
        error: (error: HttpErrorResponse) => {
          this.requests = [];

          if (error.status === 401) {
            this.errorMessage =
              'Your session has expired. Please sign in again.';
          } else if (error.status === 403) {
            this.errorMessage =
              'Only a QMS Admin can review signup requests.';
          } else if (error.status === 0) {
            this.errorMessage =
              'Cannot connect to the QMS server. Check that the backend is running.';
          } else {
            this.errorMessage =
              error.error?.message ||
              error.error?.title ||
              `Unable to load signup requests (HTTP ${error.status}).`;
          }
        }
      });
  }

  loadAdminDepartments(): void {
    this.isLoadingDepartments = true;
    this.createAccountError = '';

    this.authService
      .getAdminDepartments()
      .pipe(
        finalize(() => {
          this.isLoadingDepartments = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (departments: DepartmentOption[]) => {
          this.adminDepartments = departments ?? [];

          if (this.adminDepartments.length === 0) {
            this.createAccountError =
              'No active departments are available for account creation.';
          }
        },
        error: (error: HttpErrorResponse) => {
          this.adminDepartments = [];

          if (error.status === 401) {
            this.createAccountError =
              'Your session has expired. Please sign in again.';
          } else if (error.status === 403) {
            this.createAccountError =
              'You do not have permission to load departments.';
          } else if (error.status === 0) {
            this.createAccountError =
              'Cannot connect to the QMS server while loading departments.';
          } else {
            this.createAccountError =
              error.error?.message ||
              error.error?.title ||
              'Unable to load departments.';
          }
        }
      });
  }

  changeStatus(status: string): void {
    const allowedStatuses = [
      'Pending',
      'Approved',
      'Rejected'
    ];

    if (
      !allowedStatuses.includes(status) ||
      this.isLoading ||
      this.reviewingId !== null
    ) {
      return;
    }

    this.selectedStatus = status;
    this.loadRequests();
  }

  reviewRequest(
    request: SignupRequestItem,
    decision: 'Approve' | 'Reject'
  ): void {
    if (
      this.reviewingId !== null ||
      this.isLoading ||
      request.status !== 'Pending'
    ) {
      return;
    }

    const action =
      decision === 'Approve' ? 'approve' : 'reject';

    if (
      !window.confirm(
        `Are you sure you want to ${action} the signup request for ${request.fullName} (${request.employeeId})?`
      )
    ) {
      return;
    }

    this.reviewingId = request.id;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService
      .reviewSignupRequest(request.id, decision)
      .pipe(
        finalize(() => {
          this.reviewingId = null;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          this.successMessage =
            response.message ||
            `Signup request ${action}d successfully.`;

          this.loadRequests();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.errorMessage =
              'Your session has expired. Please sign in again.';
          } else if (error.status === 403) {
            this.errorMessage =
              'Only a QMS Admin can review signup requests.';
          } else if (error.status === 409) {
            this.errorMessage =
              error.error?.message ||
              'This request has already been reviewed.';
          } else if (error.status === 0) {
            this.errorMessage =
              'Cannot connect to the QMS server. Please try again.';
          } else {
            this.errorMessage =
              error.error?.message ||
              error.error?.title ||
              `Unable to ${action} this request.`;
          }
        }
      });
  }

  createAccount(): void {
    if (this.isCreatingAccount || this.isLoadingDepartments) {
      return;
    }

    this.createAccountError = '';
    this.createAccountSuccess = '';

    if (this.createAccountForm.invalid) {
      this.createAccountForm.markAllAsTouched();
      this.createAccountError =
        'Complete all fields and meet the password requirements.';
      return;
    }

    const form = this.createAccountForm.getRawValue();

    const request: AdminCreateUserRequest = {
      employeeId: form.employeeId.trim(),
      fullName: form.fullName.trim(),
      email: form.email.trim(),
      departmentId: Number(form.departmentId),
      role: form.role,
      password: form.password
    };

    if (
      !request.employeeId ||
      !request.fullName ||
      !request.email ||
      request.departmentId < 1
    ) {
      this.createAccountError =
        'Employee ID, full name, email and department are required.';
      return;
    }

    this.isCreatingAccount = true;

    this.authService
      .createAdminUser(request)
      .pipe(
        finalize(() => {
          this.isCreatingAccount = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (user: AdminCreatedUser) => {
          this.createAccountSuccess =
            `Active account created for ${user.fullName} (${user.employeeId}). Share the initial password securely with the employee.`;

          this.createAccountForm.reset({
            employeeId: '',
            fullName: '',
            email: '',
            departmentId: 0,
            role: 'User',
            password: ''
          });

          this.createAccountForm.markAsPristine();
          this.createAccountForm.markAsUntouched();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 400) {
            this.createAccountError =
              error.error?.message ||
              error.error?.title ||
              'The account details are invalid. Check the fields and password requirements.';
          } else if (error.status === 401) {
            this.createAccountError =
              'Your session has expired. Please sign in again.';
          } else if (error.status === 403) {
            this.createAccountError =
              'You do not have permission to create accounts.';
          } else if (error.status === 409) {
            this.createAccountError =
              error.error?.message ||
              'This employee ID or email is already registered.';
          } else if (error.status === 0) {
            this.createAccountError =
              'Cannot connect to the QMS server. Check that the backend is running.';
          } else {
            this.createAccountError =
              error.error?.message ||
              error.error?.title ||
              'Unable to create the account. Please try again.';
          }
        }
      });
  }

  goBack(): void {
    this.router.navigate(['/qms']);
  }
}
