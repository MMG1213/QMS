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
  SignupRequestItem
} from '../../services/signin.service';

import { SigninService } from '../../services/signin.service';
import { SignupService } from '../../services/signup.service';

import {
  DepartmentService,
  DepartmentOption,
  AdminDepartment
} from '../../services/admin-department.service';

import {
  AdminUsersService,
  AdminUserAccount
} from '../../services/admin-users.service';

import {
  AdminDocumentTypesService,
  AdminDocumentType
} from '../../services/admin-document-types.service';

type AccountRole = 'User' | 'Department Head';

type AdminTab =
  | 'requests'
  | 'access'
  | 'departments'
  | 'documentTypes';

@Component({
  selector: 'app-admin-signup-requests',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-signup-requests.html',
  styleUrl: './admin-signup-requests.css'
})
export class AdminSignupRequestsComponent implements OnInit {
  private readonly signinService = inject(SigninService);
  private readonly departmentService = inject(DepartmentService);
  private readonly signupService = inject(SignupService);
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly adminDocumentTypesService =
    inject(AdminDocumentTypesService);

  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  activeAdminTab: AdminTab = 'requests';

  // Signup request state
  requests: SignupRequestItem[] = [];
  selectedStatus = 'Pending';
  isLoading = false;
  reviewingId: number | null = null;
  errorMessage = '';
  successMessage = '';

  // Create account state
  adminDepartments: DepartmentOption[] = [];
  readonly accountRoles: AccountRole[] = [
    'User',
    'Department Head'
  ];

  isLoadingDepartments = false;
  isCreatingAccount = false;
  createAccountError = '';
  createAccountSuccess = '';

  // Manage access state
  adminUsers: AdminUserAccount[] = [];
  isLoadingUsers = false;
  updatingUserId: number | null = null;
  usersError = '';
  usersSuccess = '';

  // Manage departments state
  departments: AdminDepartment[] = [];
  isLoadingAllDepartments = false;
  isCreatingDepartment = false;
  updatingDepartmentId: number | null = null;
  departmentsError = '';
  departmentsSuccess = '';

  // Manage document types state
  documentTypes: AdminDocumentType[] = [];
  isLoadingDocumentTypes = false;
  isCreatingDocumentType = false;
  updatingDocumentTypeId: number | null = null;
  documentTypesError = '';
  documentTypesSuccess = '';

  private readonly passwordPattern =
    /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,128}$/;

  // --------------------------------------------------
  // FORMS
  // --------------------------------------------------

  createAccountForm = this.fb.nonNullable.group({
    employeeId: [
      '',
      [Validators.required, Validators.maxLength(50)]
    ],
    fullName: [
      '',
      [Validators.required, Validators.maxLength(150)]
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
      [Validators.required, Validators.min(1)]
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

  createDepartmentForm = this.fb.nonNullable.group({
    departmentName: [
      '',
      [Validators.required, Validators.maxLength(100)]
    ],
    departmentCode: [
      '',
      [Validators.required, Validators.maxLength(10)]
    ]
  });

  createDocumentTypeForm = this.fb.nonNullable.group({
    typeCode: [
      '',
      [Validators.required, Validators.maxLength(20)]
    ],
    typeName: [
      '',
      [Validators.required, Validators.maxLength(100)]
    ]
  });

  // --------------------------------------------------
  // INITIALIZATION
  // --------------------------------------------------

  ngOnInit(): void {
    this.loadRequests();
    this.loadAdminDepartments();
  }

  // --------------------------------------------------
  // TAB NAVIGATION
  // --------------------------------------------------

  selectAdminTab(tab: AdminTab): void {
    this.activeAdminTab = tab;

    switch (tab) {
      case 'requests':
        this.loadRequests();
        break;

      case 'access':
        this.loadAdminUsers();
        break;

      case 'departments':
        this.loadAllDepartments();
        break;

      case 'documentTypes':
        this.loadDocumentTypes();
        break;
    }
  }

  // --------------------------------------------------
  // SIGNUP REQUESTS
  // --------------------------------------------------

  loadRequests(): void {
    if (this.isLoading) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.signupService
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
          this.errorMessage = this.getErrorMessage(
            error,
            'Unable to load signup requests.'
          );
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

    this.signupService
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
          this.errorMessage = this.getErrorMessage(
            error,
            `Unable to ${action} this request.`
          );
        }
      });
  }

  // --------------------------------------------------
  // CREATE ACTIVE ACCOUNT
  // --------------------------------------------------

  loadAdminDepartments(): void {
    this.isLoadingDepartments = true;
    this.createAccountError = '';

    this.departmentService
      .getAdminDepartments()
      .pipe(
        finalize(() => {
          this.isLoadingDepartments = false;
          this.cdr.detectChanges();
        })
      )
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

    this.isCreatingAccount = true;

    this.adminUsersService
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

  // --------------------------------------------------
  // MANAGE USER ACCESS
  // --------------------------------------------------

  loadAdminUsers(): void {
    if (this.isLoadingUsers) {
      return;
    }

    this.isLoadingUsers = true;
    this.usersError = '';
    this.usersSuccess = '';

    this.adminUsersService
      .getAdminUsers()
      .pipe(
        finalize(() => {
          this.isLoadingUsers = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: users => {
          this.adminUsers = Array.isArray(users)
            ? users
            : [];
        },

        error: (error: HttpErrorResponse) => {
          this.adminUsers = [];
          this.usersError = this.getErrorMessage(
            error,
            'Unable to load user accounts.'
          );
        }
      });
  }

  toggleUserStatus(user: AdminUserAccount): void {
    if (this.updatingUserId !== null) {
      return;
    }

    const nextStatus = !user.isActive;
    const action = nextStatus
      ? 'activate'
      : 'deactivate';

    if (
      !window.confirm(
        `Are you sure you want to ${action} ${user.fullName} (${user.employeeId})?`
      )
    ) {
      return;
    }

    this.updatingUserId = user.id;
    this.usersError = '';
    this.usersSuccess = '';

    this.adminUsersService
      .updateUserStatus(user.id, nextStatus)
      .pipe(
        finalize(() => {
          this.updatingUserId = null;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          user.isActive = response.isActive;
          this.usersSuccess = response.message;
        },

        error: (error: HttpErrorResponse) => {
          this.usersError = this.getErrorMessage(
            error,
            `Unable to ${action} this account.`
          );
        }
      });
  }

  // --------------------------------------------------
  // MANAGE DEPARTMENTS
  // --------------------------------------------------

  loadAllDepartments(): void {
    if (this.isLoadingAllDepartments) {
      return;
    }

    this.isLoadingAllDepartments = true;
    this.departmentsError = '';
    this.departmentsSuccess = '';

    this.departmentService
      .getAllDepartments()
      .pipe(
        finalize(() => {
          this.isLoadingAllDepartments = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: departments => {
          this.departments = departments ?? [];
        },

        error: (error: HttpErrorResponse) => {
          this.departments = [];
          this.departmentsError = this.getErrorMessage(
            error,
            'Unable to load departments.'
          );
        }
      });
  }

  createDepartment(): void {
    if (this.isCreatingDepartment) {
      return;
    }

    this.departmentsError = '';
    this.departmentsSuccess = '';

    if (this.createDepartmentForm.invalid) {
      this.createDepartmentForm.markAllAsTouched();
      this.departmentsError =
        'Enter a department name and department code.';
      return;
    }

    const form = this.createDepartmentForm.getRawValue();

    this.isCreatingDepartment = true;

    this.departmentService
      .createDepartment({
        departmentName: form.departmentName.trim(),
        departmentCode: form.departmentCode.trim()
      })
      .pipe(
        finalize(() => {
          this.isCreatingDepartment = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          this.departmentsSuccess =
            response.message ||
            'Department created successfully.';

          this.createDepartmentForm.reset({
            departmentName: '',
            departmentCode: ''
          });

          this.createDepartmentForm.markAsPristine();
          this.createDepartmentForm.markAsUntouched();

          this.loadAllDepartments();
          this.loadAdminDepartments();
        },

        error: (error: HttpErrorResponse) => {
          this.departmentsError = this.getErrorMessage(
            error,
            'Unable to create department.'
          );
        }
      });
  }

  toggleDepartmentStatus(
    department: AdminDepartment
  ): void {
    if (this.updatingDepartmentId !== null) {
      return;
    }

    const nextStatus = !department.isActive;

    const action = nextStatus
      ? 'reactivate'
      : 'restrict';

    if (
      !window.confirm(
        `Are you sure you want to ${action} department ${department.departmentName} (${department.departmentCode})?`
      )
    ) {
      return;
    }

    this.updatingDepartmentId = department.departmentId;
    this.departmentsError = '';
    this.departmentsSuccess = '';

    this.departmentService
      .updateDepartmentStatus(
        department.departmentId,
        nextStatus
      )
      .pipe(
        finalize(() => {
          this.updatingDepartmentId = null;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          department.isActive =
            response.department.isActive;

          this.departmentsSuccess =
            response.message ||
            `Department ${action}d successfully.`;

          this.loadAdminDepartments();
        },

        error: (error: HttpErrorResponse) => {
          this.departmentsError = this.getErrorMessage(
            error,
            `Unable to ${action} department.`
          );
        }
      });
  }

  // --------------------------------------------------
  // MANAGE DOCUMENT TYPES
  // --------------------------------------------------

  loadDocumentTypes(): void {
    if (this.isLoadingDocumentTypes) {
      return;
    }

    this.isLoadingDocumentTypes = true;
    this.documentTypesError = '';
    this.documentTypesSuccess = '';

    this.adminDocumentTypesService
      .getAll()
      .pipe(
        finalize(() => {
          this.isLoadingDocumentTypes = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: types => {
          this.documentTypes = types ?? [];
        },

        error: (error: HttpErrorResponse) => {
          this.documentTypes = [];
          this.documentTypesError = this.getErrorMessage(
            error,
            'Unable to load document types.'
          );
        }
      });
  }

  createDocumentType(): void {
    if (this.isCreatingDocumentType) {
      return;
    }

    this.documentTypesError = '';
    this.documentTypesSuccess = '';

    if (this.createDocumentTypeForm.invalid) {
      this.createDocumentTypeForm.markAllAsTouched();
      this.documentTypesError =
        'Enter a document type code and name.';
      return;
    }

    const form = this.createDocumentTypeForm.getRawValue();

    this.isCreatingDocumentType = true;

    this.adminDocumentTypesService
      .create({
        typeCode: form.typeCode.trim(),
        typeName: form.typeName.trim()
      })
      .pipe(
        finalize(() => {
          this.isCreatingDocumentType = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          this.documentTypesSuccess =
            response.message ||
            'Document type created successfully.';

          this.createDocumentTypeForm.reset({
            typeCode: '',
            typeName: ''
          });

          this.createDocumentTypeForm.markAsPristine();
          this.createDocumentTypeForm.markAsUntouched();

          this.loadDocumentTypes();
        },

        error: (error: HttpErrorResponse) => {
          this.documentTypesError = this.getErrorMessage(
            error,
            'Unable to create document type.'
          );
        }
      });
  }

  toggleDocumentTypeStatus(
    documentType: AdminDocumentType
  ): void {
    if (this.updatingDocumentTypeId !== null) {
      return;
    }

    const nextStatus = !documentType.isActive;

    const action = nextStatus
      ? 'reactivate'
      : 'restrict';

    if (
      !window.confirm(
        `Are you sure you want to ${action} document type ${documentType.typeName} (${documentType.typeCode})?`
      )
    ) {
      return;
    }

    this.updatingDocumentTypeId =
      documentType.documentTypeId;

    this.documentTypesError = '';
    this.documentTypesSuccess = '';

    this.adminDocumentTypesService
      .updateStatus(
        documentType.documentTypeId,
        nextStatus
      )
      .pipe(
        finalize(() => {
          this.updatingDocumentTypeId = null;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          documentType.isActive =
            response.documentType.isActive;

          this.documentTypesSuccess =
            response.message ||
            `Document type ${action}d successfully.`;
        },

        error: (error: HttpErrorResponse) => {
          this.documentTypesError = this.getErrorMessage(
            error,
            `Unable to ${action} document type.`
          );
        }
      });
  }

  // --------------------------------------------------
  // SHARED ERROR HANDLING
  // --------------------------------------------------

  private getErrorMessage(
    error: HttpErrorResponse,
    fallback: string
  ): string {
    if (error.status === 401) {
      return 'Your session has expired or is invalid. Please sign in again.';
    }

    if (error.status === 403) {
      return 'Access denied. This feature is restricted to QMS Admin.';
    }

    if (error.status === 0) {
      return 'Cannot connect to the QMS server. Check that the backend is running.';
    }

    return error.error?.message ||
      error.error?.title ||
      fallback;
  }

  // --------------------------------------------------
  // NAVIGATION
  // --------------------------------------------------

  goBack(): void {
    this.router.navigate(['/qms']);
  }
}