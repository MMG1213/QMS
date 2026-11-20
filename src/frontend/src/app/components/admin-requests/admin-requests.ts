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
  FormsModule,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';

import {
  AdminCreateUserRequest,
  AdminCreatedUser,
  SignupRequestItem
} from '../../services/signin.service';
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

import {
  DepartmentApprovalMappingService,
  DepartmentApprovalMappingDto,
  CreateDepartmentApprovalMappingDto,
  UpdateDepartmentApprovalMappingDto,
  CreateApprovalLevelDto,
  ApprovalMappingEmployeeDto
} from '../../services/admin-department-approval-mapping.service';

type AdminTab =
  | 'requests'
  | 'access'
  | 'departments'
  | 'documentTypes'
  | 'approvalMappings';

@Component({
  selector: 'app-admin-requests',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './admin-requests.html',
  styleUrl: './admin-requests.css'
})
export class AdminSignupRequestsComponent implements OnInit {
  private readonly departmentService = inject(DepartmentService);
  private readonly signupService = inject(SignupService);
  private readonly adminUsersService = inject(AdminUsersService);
  private readonly adminDocumentTypesService =
    inject(AdminDocumentTypesService);
  private readonly approvalMappingService =
    inject(DepartmentApprovalMappingService);

  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly cdr = inject(ChangeDetectorRef);

  activeAdminTab: AdminTab = 'access';

  // Manage signup requests state
  requests: SignupRequestItem[] = [];
  selectedStatus = 'Pending';
  isLoading = false;
  reviewingId: number | null = null;
  errorMessage = '';
  successMessage = '';
  adminDepartments: DepartmentOption[] = [];

  // Create Active Account form
  readonly accountRoles: ('User' | 'Department Head')[] = ['User', 'Department Head'];
  isLoadingDepartments = false;
  isCreatingAccount = false;
  createAccountError = '';
  createAccountSuccess = '';

  private readonly passwordPattern =
    /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,128}$/;

  createAccountForm = this.fb.nonNullable.group({
    employeeId: ['', [Validators.required, Validators.maxLength(50)]],
    fullName: ['', [Validators.required, Validators.maxLength(150)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    departmentId: [0, [Validators.required, Validators.min(1)]],
    role: ['User' as 'User' | 'Department Head', Validators.required],
    password: ['', [
      Validators.required,
      Validators.minLength(8),
      Validators.maxLength(128),
      Validators.pattern(this.passwordPattern)
    ]]
  });

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

  // Department approval mappings state
  approvalMappings: DepartmentApprovalMappingDto[] = [];
  approvalMappingEmployees: ApprovalMappingEmployeeDto[] = [];
  isLoadingApprovalMappings = false;
  isLoadingMappingEmployees = false;
  isSavingApprovalMapping = false;
  approvalMappingsError = '';
  approvalMappingsSuccess = '';
  selectedMappingDepartmentId = 0;
  selectedCreatorEmployeeIds: string[] = [];
  editingApprovalMappingId: number | null = null;
  approvalMappingIsActive = true;
  approvalMappingApprovalLevels: CreateApprovalLevelDto[] = [];
  readonly approvalTypes = ['Department', 'QMSFinal'];

  // Simple approval-mapping wizard state
  approvalWizardStep = 1;
  finalApprovalEmployeeId = '';
  creatorDropdownSelection = '';
  departmentSearchText = '';
  creatorSearchText = '';
  finalApproverSearchText = '';
  approvalLevelSearchTexts: string[] = [];

  showDepartmentSearch = false;
  showCreatorSearch = false;
  showFinalApproverSearch = false;
  approvalLevelSearchOpen: boolean[] = [];

  toggleSearch(target: 'department' | 'creator' | 'final'): void {
    if (target === 'department') this.showDepartmentSearch = !this.showDepartmentSearch;
    if (target === 'creator') this.showCreatorSearch = !this.showCreatorSearch;
    if (target === 'final') this.showFinalApproverSearch = !this.showFinalApproverSearch;
  }

  toggleApprovalLevelSearch(index: number): void {
    this.approvalLevelSearchOpen[index] = !this.approvalLevelSearchOpen[index];
  }

  goToApprovalWizardStep(step: number): void {
    if (step < 1 || step > 4 || this.isSavingApprovalMapping || this.isLoadingMappingEmployees) return;
    if (step > 1 && this.selectedMappingDepartmentId <= 0) {
      this.approvalMappingsError = 'Choose a department first.';
      this.approvalWizardStep = 1;
      return;
    }
    if (step > 2 && !this.selectedCreatorEmployeeIds.length) {
      this.approvalMappingsError = 'Choose at least one DCR creator.';
      this.approvalWizardStep = 2;
      return;
    }
    if (step > 3 && (
      !this.approvalMappingApprovalLevels.length ||
      this.approvalMappingApprovalLevels.some(level => !level.approverEmployeeId)
    )) {
      this.approvalMappingsError = 'Choose an approver for every approval level.';
      this.approvalWizardStep = 3;
      return;
    }
    this.approvalMappingsError = '';
    this.approvalWizardStep = step;
  }

  get departmentCreatorEmployees(): ApprovalMappingEmployeeDto[] {
    // DCR creators must belong to the selected department.
    return this.approvalMappingEmployees.filter(employee =>
      employee.departmentId === this.selectedMappingDepartmentId
    );
  }

  // Dynamic columns for the existing mappings table (excluding QMSFinal).
  get mappingApprovalLevelNumbers(): number[] {
    const maxLevel = this.approvalMappings.reduce((max, mapping) => {
      const departmentLevels = mapping.approvalLevels
        .filter(level => level.approvalType !== 'QMSFinal')
        .map(level => level.levelNumber);
      return Math.max(max, ...departmentLevels, 0);
    }, 0);
    return Array.from({ length: maxLevel }, (_, index) => index + 1);
  }

  approvalLevelForMapping(
    mapping: DepartmentApprovalMappingDto,
    levelNumber: number
  ) {
    return mapping.approvalLevels.find(level =>
      level.approvalType !== 'QMSFinal' && level.levelNumber === levelNumber
    );
  }

  finalApprovalForMapping(mapping: DepartmentApprovalMappingDto) {
    return mapping.approvalLevels.find(level => level.approvalType === 'QMSFinal');
  }

  get filteredDepartments(): typeof this.adminDepartments {
    const query = this.departmentSearchText.trim().toLowerCase();
    return this.adminDepartments.filter(department =>
      !query || department.name.toLowerCase().includes(query)
    );
  }

  filterEmployeeOptions(employees: ApprovalMappingEmployeeDto[], searchText: string): ApprovalMappingEmployeeDto[] {
    const query = (searchText || '').trim().toLowerCase();
    if (!query) return employees;
    return employees.filter(employee => {
      const employeeId = (employee.employeeId || '').toLowerCase();
      const fullName = (employee.fullName || '').toLowerCase();
      const role = (employee.role || '').toLowerCase();
      return employeeId.includes(query) || fullName.includes(query) || role.includes(query);
    });
  }

  setApprovalLevelSearch(index: number, value: string): void {
    this.approvalLevelSearchTexts[index] = value;
  }

  get selectedCreatorEmployees(): ApprovalMappingEmployeeDto[] {
    return this.selectedCreatorEmployeeIds
      .map(id => this.approvalMappingEmployees.find(employee => employee.employeeId === id))
      .filter((employee): employee is ApprovalMappingEmployeeDto => !!employee);
  }

  availableApproversForLevel(index: number): ApprovalMappingEmployeeDto[] {
    const current = this.approvalMappingApprovalLevels[index];
    const assigned = new Set<string>(this.selectedCreatorEmployeeIds);
    this.approvalMappingApprovalLevels.forEach((level, i) => {
      if (i !== index && level.approverEmployeeId) assigned.add(level.approverEmployeeId);
    });
    if (this.finalApprovalEmployeeId) assigned.add(this.finalApprovalEmployeeId);
    return this.approvalMappingEmployees.filter(employee =>
      (!assigned.has(employee.employeeId) || employee.employeeId === current?.approverEmployeeId)
    );
  }

  availableFinalApprovers(): ApprovalMappingEmployeeDto[] {
    const assigned = new Set<string>(this.selectedCreatorEmployeeIds);
    this.approvalMappingApprovalLevels.forEach(level => {
      if (level.approverEmployeeId) assigned.add(level.approverEmployeeId);
    });
    return this.approvalMappingEmployees.filter(employee =>
      (!assigned.has(employee.employeeId) || employee.employeeId === this.finalApprovalEmployeeId)
    );
  }

  selectedApproverForLevel(index: number): ApprovalMappingEmployeeDto | undefined {
    const employeeId = this.approvalMappingApprovalLevels[index]?.approverEmployeeId;
    return this.approvalMappingEmployees.find(employee => employee.employeeId === employeeId);
  }

  selectedFinalApprover(): ApprovalMappingEmployeeDto | undefined {
    return this.approvalMappingEmployees.find(employee => employee.employeeId === this.finalApprovalEmployeeId);
  }

  nextApprovalWizardStep(): void {
    this.approvalMappingsError = '';
    if (this.approvalWizardStep === 1 && this.selectedMappingDepartmentId <= 0) { this.approvalMappingsError = 'Choose a department first.'; return; }
    if (this.approvalWizardStep === 2 && !this.selectedCreatorEmployeeIds.length) { this.approvalMappingsError = 'Choose at least one DCR creator.'; return; }
    if (this.approvalWizardStep === 3) {
      if (!this.approvalMappingApprovalLevels.length) { this.approvalMappingsError = 'Add at least one approval level.'; return; }
      if (this.approvalMappingApprovalLevels.some(level => !level.approverEmployeeId)) { this.approvalMappingsError = 'Choose an approver for every level.'; return; }
    }
    if (this.approvalWizardStep < 4) this.approvalWizardStep++;
  }

  previousApprovalWizardStep(): void { if (this.approvalWizardStep > 1) this.approvalWizardStep--; }

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
    // Load departments used by the approval-mapping wizard.
    this.loadAdminDepartments();

    // Keep the visible tab synchronized with the current child route.
    this.route.data.subscribe(data => {
      // The dedicated URL must open the signup requests tab even if route data
      // inherited from the parent route contains another admin tab.
      const currentUrl = this.router.url.toLowerCase();
      const routeTab = currentUrl.includes('/manage-signup-requests')
        ? 'requests'
        : data['adminTab'] as AdminTab | undefined;
      const allowedTabs: AdminTab[] = [
        'requests',
        'access',
        'departments',
        'documentTypes',
        'approvalMappings'
      ];

      const tab: AdminTab = routeTab && allowedTabs.includes(routeTab)
        ? routeTab
        : 'access';

      this.activateAdminTab(tab);
    });
  }

  // --------------------------------------------------
  // TAB NAVIGATION
  // --------------------------------------------------

  // Switch tabs locally. This keeps the authenticated admin route intact and
  // avoids child-route guard redirects to the login page.
  selectAdminTab(tab: AdminTab): void {
    this.activateAdminTab(tab);
  }

  // Called when Angular Router data changes to display/load the selected tab.
  private activateAdminTab(tab: AdminTab): void {
    this.activeAdminTab = tab;

    switch (tab) {
      case 'requests':
        this.loadRequests();
        this.loadAccountDepartments();
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

      case 'approvalMappings':
        this.loadApprovalMappings();
        break;
    }
  }

  // --------------------------------------------------
  // CREATE ACTIVE ACCOUNT
  // --------------------------------------------------

  loadAdminDepartments(): void {
    this.departmentService
      .getAdminDepartments()
      .pipe(finalize(() => this.cdr.detectChanges()))
      .subscribe({
        next: departments => {
          this.adminDepartments = departments ?? [];
        },
        error: (error: HttpErrorResponse) => {
          this.adminDepartments = [];
          console.error(
            this.getErrorMessage(error, 'Unable to load departments for approval mappings.')
          );
        }
      });
  }



  loadAccountDepartments(): void {
    this.isLoadingDepartments = true;
    this.createAccountError = '';
    this.departmentService.getAdminDepartments()
      .pipe(finalize(() => {
        this.isLoadingDepartments = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: departments => {
          this.adminDepartments = departments ?? [];
          if (!this.adminDepartments.length) {
            this.createAccountError = 'No active departments are available for account creation.';
          }
        },
        error: (error: HttpErrorResponse) => {
          this.adminDepartments = [];
          this.createAccountError = this.getErrorMessage(error, 'Unable to load departments.');
        }
      });
  }

  createAccount(): void {
    if (this.isCreatingAccount || this.isLoadingDepartments) return;
    this.createAccountError = '';
    this.createAccountSuccess = '';

    if (this.createAccountForm.invalid) {
      this.createAccountForm.markAllAsTouched();
      this.createAccountError = 'Complete all fields and meet the password requirements.';
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
    this.adminUsersService.createAdminUser(request)
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
          this.createAccountError = this.getErrorMessage(error, 'Unable to create the account.');
        }
      });
  }

  // --------------------------------------------------
  // MANAGE SIGNUP REQUESTS
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
  // DEPARTMENT APPROVAL MAPPINGS
  // --------------------------------------------------

  loadApprovalMappings(): void {
    if (this.isLoadingApprovalMappings) return;
    this.isLoadingApprovalMappings = true;
    this.approvalMappingsError = '';
    this.approvalMappingsSuccess = '';
    this.approvalMappingService.getAllMappings().pipe(finalize(() => {
      this.isLoadingApprovalMappings = false;
      this.cdr.detectChanges();
    })).subscribe({
      next: response => this.approvalMappings = Array.isArray(response) ? response : [],
      error: (error: HttpErrorResponse) => {
        this.approvalMappings = [];
        this.approvalMappingsError = this.getErrorMessage(error, 'Unable to load department approval mappings.');
      }
    });
  }

  onMappingDepartmentChange(value: string | number): void {
    this.selectedMappingDepartmentId = Number(value);
    this.selectedCreatorEmployeeIds = [];
    this.creatorDropdownSelection = '';
    this.departmentSearchText = '';
    this.creatorSearchText = '';
    this.finalApproverSearchText = '';
    this.approvalLevelSearchTexts = [];
    this.showDepartmentSearch = false;
    this.showCreatorSearch = false;
    this.showFinalApproverSearch = false;
    this.approvalLevelSearchOpen = [];
    this.approvalMappingEmployees = [];
    this.approvalMappingApprovalLevels = [];
    this.editingApprovalMappingId = null;
    this.approvalMappingIsActive = true;
    this.approvalWizardStep = 1;
    this.finalApprovalEmployeeId = '';
    if (this.selectedMappingDepartmentId > 0) this.loadMappingEmployees(this.selectedMappingDepartmentId);
  }

  loadMappingEmployees(_departmentId: number): void {
    this.isLoadingMappingEmployees = true;
    this.approvalMappingsError = '';
    this.adminUsersService.getAdminUsers().pipe(finalize(() => {
      this.isLoadingMappingEmployees = false;
      this.cdr.detectChanges();
    })).subscribe({
      next: users => {
        const allUsers = (Array.isArray(users) ? users : []) as unknown as Array<{
          employeeId: string; fullName: string; role: string; departmentId: number | null; isActive?: boolean;
        }>;
        this.approvalMappingEmployees = allUsers.filter(user => user.isActive !== false).map(user => ({
          employeeId: user.employeeId,
          fullName: user.fullName,
          role: user.role,
          departmentId: user.departmentId
        } as ApprovalMappingEmployeeDto));
      },
      error: (error: HttpErrorResponse) => {
        this.approvalMappingEmployees = [];
        this.approvalMappingsError = this.getErrorMessage(error, 'Unable to load employee login IDs.');
      }
    });
  }

  onCreatorDropdownChange(value: string): void {
    this.creatorDropdownSelection = value;
    if (value && !this.selectedCreatorEmployeeIds.includes(value)) {
      this.selectedCreatorEmployeeIds = [...this.selectedCreatorEmployeeIds, value];
    }
    this.creatorDropdownSelection = '';
  }

  removeCreator(employeeId: string): void {
    this.selectedCreatorEmployeeIds = this.selectedCreatorEmployeeIds.filter(id => id !== employeeId);
  }

  addApprovalLevel(): void {
    this.approvalLevelSearchOpen.push(false);
    this.approvalLevelSearchTexts.push('');
    this.approvalMappingApprovalLevels.push({
      levelNumber: this.approvalMappingApprovalLevels.length + 1,
      approvalType: 'Department',
      approverEmployeeId: '',
      redirectedApproverEmployeeId: null,
      isActive: true
    });
  }

  removeApprovalLevel(index: number): void {
    this.approvalLevelSearchOpen.splice(index, 1);
    this.approvalLevelSearchTexts.splice(index, 1);
    this.approvalMappingApprovalLevels.splice(index, 1);
    this.approvalMappingApprovalLevels = this.approvalMappingApprovalLevels.map((level, i) => ({
      ...level,
      levelNumber: i + 1
    }));
  }

  editApprovalMapping(mapping: DepartmentApprovalMappingDto): void {
    this.approvalMappingsError = '';
    this.approvalMappingsSuccess = '';
    this.editingApprovalMappingId = mapping.id;
    this.selectedMappingDepartmentId = mapping.departmentId;
    this.approvalMappingIsActive = mapping.isActive;
    this.selectedCreatorEmployeeIds = mapping.dcrCreators.map(c => c.creatorEmployeeId);
    this.creatorDropdownSelection = '';
    const finalLevel = mapping.approvalLevels.find(level => level.approvalType === 'QMSFinal');
    this.approvalMappingApprovalLevels = mapping.approvalLevels
      .filter(level => level.approvalType !== 'QMSFinal')
      .map((level, index) => ({
        levelNumber: index + 1, approvalType: 'Department',
        approverEmployeeId: level.approverEmployeeId,
        redirectedApproverEmployeeId: null,
        isActive: level.isActive
      }));
    this.approvalWizardStep = 3;
    this.finalApprovalEmployeeId = finalLevel?.approverEmployeeId ?? '';
    this.loadMappingEmployees(mapping.departmentId);
  }

  resetApprovalMappingForm(): void {
    this.editingApprovalMappingId = null;
    this.selectedMappingDepartmentId = 0;
    this.selectedCreatorEmployeeIds = [];
    this.approvalMappingEmployees = [];
    this.approvalMappingApprovalLevels = [];
    this.approvalMappingIsActive = true;
    this.approvalWizardStep = 1;
    this.finalApprovalEmployeeId = '';
    this.approvalMappingsError = '';
  }

  viewApprovalMapping(mapping: DepartmentApprovalMappingDto): void {
    const creators = mapping.dcrCreators.map(c => `${c.creatorEmployeeName} (${c.creatorEmployeeId})`).join('\n') || 'None';
    const levels = mapping.approvalLevels.map(l => `Level ${l.levelNumber}: ${l.approverEmployeeName} (${l.approverEmployeeId}) — ${l.approvalType}`).join('\n') || 'None';
    window.alert(`Department: ${mapping.departmentName}\nVersion: ${mapping.configVersion}\nStatus: ${mapping.isActive ? 'Active' : 'Inactive'}\n\nDCR Creators:\n${creators}\n\nApproval Levels:\n${levels}`);
  }

  saveApprovalMapping(): void {
    if (this.isSavingApprovalMapping) return;
    this.approvalMappingsError = '';
    this.approvalMappingsSuccess = '';
    if (this.selectedMappingDepartmentId <= 0) { this.approvalMappingsError = 'Select a department.'; return; }
    if (!this.selectedCreatorEmployeeIds.length) { this.approvalMappingsError = 'Select at least one DCR creator.'; return; }
    const creatorOutsideDepartment = this.selectedCreatorEmployeeIds.some(id =>
      !this.departmentCreatorEmployees.some(employee => employee.employeeId === id)
    );
    if (creatorOutsideDepartment) { this.approvalMappingsError = 'Every DCR creator must belong to the selected department.'; return; }
    if (!this.approvalMappingApprovalLevels.length) { this.approvalMappingsError = 'Add at least one approval level.'; return; }
    if (this.approvalMappingApprovalLevels.some(l => !l.approverEmployeeId.trim())) { this.approvalMappingsError = 'Select an approver for every approval level.'; return; }
    if (!this.finalApprovalEmployeeId) { this.approvalMappingsError = 'Select the final approver in Step 4.'; return; }
    const ids = [...this.selectedCreatorEmployeeIds, ...this.approvalMappingApprovalLevels.map(level => level.approverEmployeeId), this.finalApprovalEmployeeId].filter(Boolean);
    if (new Set(ids).size !== ids.length) { this.approvalMappingsError = 'An employee can be selected only once in the workflow.'; return; }
    const levels: CreateApprovalLevelDto[] = this.approvalMappingApprovalLevels.map((l, i) => ({
      levelNumber: i + 1, approvalType: l.approvalType, approverEmployeeId: l.approverEmployeeId,
      redirectedApproverEmployeeId: null, isActive: l.isActive
    }));
    levels.push({ levelNumber: levels.length + 1, approvalType: 'QMSFinal', approverEmployeeId: this.finalApprovalEmployeeId, redirectedApproverEmployeeId: null, isActive: true });
    this.isSavingApprovalMapping = true;
    const finish = () => { this.isSavingApprovalMapping = false; this.cdr.detectChanges(); };
    if (this.editingApprovalMappingId === null) {
      const request: CreateDepartmentApprovalMappingDto = { departmentId: this.selectedMappingDepartmentId, creatorEmployeeIds: this.selectedCreatorEmployeeIds, approvalLevels: levels };
      this.approvalMappingService.createMapping(request).pipe(finalize(finish)).subscribe({
        next: result => { this.approvalMappingsSuccess = `Mapping created for ${result.departmentName}.`; this.resetApprovalMappingForm(); this.loadApprovalMappings(); },
        error: (error: HttpErrorResponse) => this.approvalMappingsError = this.getErrorMessage(error, 'Unable to create approval mapping.')
      });
    } else {
      const request: UpdateDepartmentApprovalMappingDto = { isActive: this.approvalMappingIsActive, creatorEmployeeIds: this.selectedCreatorEmployeeIds, approvalLevels: levels };
      this.approvalMappingService.updateMapping(this.editingApprovalMappingId, request).pipe(finalize(finish)).subscribe({
        next: result => { this.approvalMappingsSuccess = `Mapping updated for ${result.departmentName}.`; this.resetApprovalMappingForm(); this.loadApprovalMappings(); },
        error: (error: HttpErrorResponse) => this.approvalMappingsError = this.getErrorMessage(error, 'Unable to update approval mapping.')
      });
    }
  }

  toggleApprovalMappingStatus(mapping: DepartmentApprovalMappingDto): void {
    if (this.isSavingApprovalMapping) return;
    const nextActive = !mapping.isActive;
    const request: UpdateDepartmentApprovalMappingDto = {
      isActive: nextActive,
      creatorEmployeeIds: mapping.dcrCreators.map(c => c.creatorEmployeeId),
      approvalLevels: mapping.approvalLevels.map(l => ({
        levelNumber: l.levelNumber, approvalType: l.approvalType, approverEmployeeId: l.approverEmployeeId,
        redirectedApproverEmployeeId: null, isActive: l.isActive
      }))
    };
    this.isSavingApprovalMapping = true;
    this.approvalMappingService.updateMapping(mapping.id, request).pipe(finalize(() => { this.isSavingApprovalMapping = false; this.cdr.detectChanges(); })).subscribe({
      next: updated => { mapping.isActive = updated.isActive; this.approvalMappingsSuccess = `Mapping ${nextActive ? 'activated' : 'deactivated'} successfully.`; },
      error: (error: HttpErrorResponse) => this.approvalMappingsError = this.getErrorMessage(error, 'Unable to change mapping status.')
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
