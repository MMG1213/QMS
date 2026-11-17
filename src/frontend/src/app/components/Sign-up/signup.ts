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
import { finalize } from 'rxjs';

import {
  AdminCreateUserRequest,
  AdminCreatedUser,
  SignupRequestItem
} from '../../services/signin.service';

import { SignupService } from '../../services/signup.service';
import { DepartmentService, DepartmentOption } from '../../services/admin-department.service';
import { AdminUsersService } from '../../services/admin-users.service';

type AccountRole = 'User' | 'Department Head';

@Component({
  selector: 'app-signup-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class SignupManagementComponent implements OnInit {
  private readonly signupService = inject(SignupService);
  private readonly departmentService = inject(DepartmentService);
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly fb = inject(FormBuilder);
  private readonly cdr = inject(ChangeDetectorRef);

  // Signup request review
  requests: SignupRequestItem[] = [];
  selectedStatus = 'Pending';
  isLoading = false;
  reviewingId: number | null = null;
  errorMessage = '';
  successMessage = '';

  // Create active account
  adminDepartments: DepartmentOption[] = [];
  readonly accountRoles: AccountRole[] = ['User', 'Department Head'];

  isLoadingDepartments = false;
  isCreatingAccount = false;
  createAccountError = '';
  createAccountSuccess = '';

  private readonly passwordPattern =
    /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,128}$/;

  createAccountForm = this.fb.nonNullable.group({
    employeeId: ['', [Validators.required, Validators.maxLength(50)]],
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [
      Validators.required,
      Validators.email,
      Validators.maxLength(256)
    ]],
    departmentId: [0, [Validators.required, Validators.min(1)]],
    role: ['User' as AccountRole, Validators.required],
    password: ['', [
      Validators.required,
      Validators.minLength(8),
      Validators.maxLength(128),
      Validators.pattern(this.passwordPattern)
    ]]
  });

  ngOnInit(): void {
    this.loadRequests();
    this.loadAdminDepartments();
  }

  loadRequests(): void {
    if (this.isLoading) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.signupService
      .getSignupRequests(this.selectedStatus)
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
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
          this.errorMessage = this.getErrorMessage(
            error,
            'Unable to load signup requests.'
          );
        }
      });
  }

  changeStatus(status: string): void {
    const allowedStatuses = ['Pending', 'Approved', 'Rejected'];

    if (
      !allowedStatuses.includes(status) ||
      this.isLoading ||
      this.reviewingId !== null
    ) return;

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
    ) return;

    const action = decision === 'Approve' ? 'approve' : 'reject';

    if (!window.confirm(
      `Are you sure you want to ${action} the signup request for ${request.fullName} (${request.employeeId})?`
    )) return;

    this.reviewingId = request.id;
    this.errorMessage = '';
    this.successMessage = '';

    this.signupService
      .reviewSignupRequest(request.id, decision)
      .pipe(finalize(() => {
        this.reviewingId = null;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          this.successMessage =
            response.message || `Signup request ${action}d successfully.`;
          this.loadRequests();
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.getErrorMessage(
            error,
            `Unable to ${action} this request.`
          );
        }
      });
  }

  loadAdminDepartments(): void {
    this.isLoadingDepartments = true;
    this.createAccountError = '';

    this.departmentService
      .getAdminDepartments()
      .pipe(finalize(() => {
        this.isLoadingDepartments = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: departments => {
          this.adminDepartments = departments ?? [];
          if (this.adminDepartments.length === 0) {
            this.createAccountError =
              'No active departments are available for account creation.';
          }
        },
        error: (error: HttpErrorResponse) => {
          this.adminDepartments = [];
          this.createAccountError = this.getErrorMessage(
            error,
            'Unable to load departments.'
          );
        }
      });
  }

  createAccount(): void {
    if (this.isCreatingAccount || this.isLoadingDepartments) return;

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

    this.isCreatingAccount = true;

    this.adminUsersService
      .createAdminUser(request)
      .pipe(finalize(() => {
        this.isCreatingAccount = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: (user: AdminCreatedUser) => {
          this.createAccountSuccess =
            `Active account created for ${user.fullName} (${user.employeeId}).`;

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
          this.createAccountError = this.getErrorMessage(
            error,
            'Unable to create the account.'
          );
        }
      });
  }

  private getErrorMessage(error: HttpErrorResponse, fallback: string): string {
    if (error.status === 401) {
      return 'Your session has expired or is invalid. Please sign in again.';
    }
    if (error.status === 403) {
      return 'Access denied. This feature is restricted to QMS Admin.';
    }
    if (error.status === 0) {
      return 'Cannot connect to the QMS server. Check that the backend is running.';
    }
    return error.error?.message || error.error?.title || fallback;
  }
}
