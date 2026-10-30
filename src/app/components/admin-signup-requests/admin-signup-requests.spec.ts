
import { CommonModule } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
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
  SigninService,
  DepartmentOption,
  SignupRequestItem
} from '../../services/auth.service';

@Component({
  selector: 'app-admin-signup-requests',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-signup-requests.html',
  styleUrl: './admin-signup-requests.css'
})
export class AdminSignupRequestsComponent implements OnInit {
  private readonly SigninService = inject(SigninService);
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  requests: SignupRequestItem[] = [];
  adminDepartments: DepartmentOption[] = [];

  readonly accountRoles: Array<'User' | 'Department Head'> = [
    'User',
    'Department Head'
  ];

  selectedStatus = 'Pending';
  isLoading = false;
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
      'User' as 'User' | 'Department Head',
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
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.SigninService
      .getSignupRequests(this.selectedStatus)
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (requests: SignupRequestItem[]) => {
          this.requests = requests ?? [];
        },
        error: (error: HttpErrorResponse) => {
          this.requests = [];

          if (error.status === 401) {
            this.errorMessage =
              'Your session has expired. Please sign in again.';
          } else if (error.status === 403) {
            this.errorMessage =
              'You do not have permission to view signup requests.';
          } else if (error.status === 0) {
            this.errorMessage =
              'Unable to connect to the QMS server. Check that the backend is running.';
          } else {
            this.errorMessage =
              error.error?.message ||
              error.error?.title ||
              'Unable to load signup requests. Please try again.';
          }
        }
      });
  }

  loadAdminDepartments(): void {
    this.createAccountError = '';

    this.SigninService.getAdminDepartments().subscribe({
      next: (departments: DepartmentOption[]) => {
        this.adminDepartments = departments ?? [];

        if (this.adminDepartments.length === 0) {
          this.createAccountError =
            'No active departments are available. Add or activate a department before creating an account.';
        }

        this.cdr.detectChanges();
      },
      error: (error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.createAccountError =
            'Your session has expired. Please sign in again.';
        } else if (error.status === 403) {
          this.createAccountError =
            'You do not have permission to load departments.';
        } else if (error.status === 0) {
          this.createAccountError =
            'Unable to connect to the server while loading departments.';
        } else {
          this.createAccountError =
            error.error?.message ||
            error.error?.title ||
            'Unable to load departments.';
        }

        this.cdr.detectChanges();
      }
    });
  }

  changeStatus(status: string): void {
    const allowedStatuses = [
      'Pending',
      'Approved',
      'Rejected'
    ];

    if (!allowedStatuses.includes(status)) {
      return;
    }

    this.selectedStatus = status;
    this.loadRequests();
  }

  reviewRequest(
    request: SignupRequestItem,
    decision: 'Approve' | 'Reject'
  ): void {
    if (this.reviewingId !== null || request.status !== 'Pending') {
      return;
    }

    const action = decision === 'Approve' ? 'approve' : 'reject';
    const confirmed = window.confirm(
      `Are you sure you want to ${action} the signup request for ${request.fullName} (${request.employeeId})?`
    );

    if (!confirmed) {
      return;
    }

    this.reviewingId = request.id;
    this.errorMessage = '';
    this.successMessage = '';

    this.SigninService
      .reviewSignupRequest(request.id, decision)
      .pipe(
        finalize(() => {
          this.reviewingId = null;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: () => {
          this.successMessage =
            decision === 'Approve'
              ? `Signup request for ${request.fullName} approved successfully.`
              : `Signup request for ${request.fullName} rejected.`;

          this.loadRequests();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.errorMessage =
              'Your session has expired. Please sign in again.';
          } else if (error.status === 403) {
            this.errorMessage =
              'You do not have permission to review signup requests.';
          } else if (error.status === 0) {
            this.errorMessage =
              'Unable to connect to the QMS server. Please try again.';
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
    if (this.isCreatingAccount) {
      return;
    }

    this.createAccountError = '';
    this.createAccountSuccess = '';

    if (this.createAccountForm.invalid) {
      this.createAccountForm.markAllAsTouched();
      this.createAccountError =
        'Please complete all fields correctly and follow the password requirements.';
      return;
    }

    const formValue = this.createAccountForm.getRawValue();

    const request: AdminCreateUserRequest = {
      employeeId: formValue.employeeId.trim(),
      fullName: formValue.fullName.trim(),
      email: formValue.email.trim(),
      departmentId: Number(formValue.departmentId),
      role: formValue.role,
      password: formValue.password
    };

    if (
      !request.employeeId ||
      !request.fullName ||
      !request.email ||
      !request.departmentId
    ) {
      this.createAccountError =
        'Employee ID, full name, email, and department are required.';
      return;
    }

    this.isCreatingAccount = true;

    this.SigninService
      .createAdminUser(request)
      .pipe(
        finalize(() => {
          this.isCreatingAccount = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (createdUser: AdminCreatedUser) => {
          this.createAccountSuccess =
            `Active account created successfully for ${createdUser.fullName} (${createdUser.employeeId}). Share the initial password securely with the employee.`;

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
          this.cdr.detectChanges();
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 400) {
            this.createAccountError =
              error.error?.message ||
              error.error?.title ||
              'The account details are invalid. Check the employee ID, email, department, and password.';
          } else if (error.status === 401) {
            this.createAccountError =
              'Your session has expired. Please sign in again.';
          } else if (error.status === 403) {
            this.createAccountError =
              'You do not have permission to create accounts.';
          } else if (error.status === 409) {
            this.createAccountError =
              error.error?.message ||
              'An account with this employee ID or email may already exist.';
          } else if (error.status === 0) {
            this.createAccountError =
              'Unable to connect to the QMS server. Check that the backend is running.';
          } else {
            this.createAccountError =
              error.error?.message ||
              error.error?.title ||
              'Unable to create the account. Please try again.';
          }

          this.cdr.detectChanges();
        }
      });
  }

  goBack(): void {
    this.router.navigate(['/qms']);
  }
}
