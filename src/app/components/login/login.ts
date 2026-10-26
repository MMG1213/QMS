
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { finalize, timeout } from 'rxjs';

import {
  AuthService,
  DepartmentOption
} from '../../services/auth.service';

type PageMode = 'login' | 'forgot' | 'signup';
type SignupRole = 'User' | 'Department Head';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  mode: PageMode = 'login';
  showPassword = false;
  isLoading = false;
  isLoadingDepartments = false;

  message = '';
  messageType: 'error' | 'success' = 'error';

  departments: DepartmentOption[] = [];

  readonly signupRoles: SignupRole[] = [
    'User',
    'Department Head'
  ];

  private readonly passwordPattern =
    /^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,128}$/;

  loginForm = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  forgotForm = this.fb.nonNullable.group({
    email: [
      '',
      [
        Validators.required,
        Validators.email,
        Validators.maxLength(256)
      ]
    ]
  });

  signupForm = this.fb.nonNullable.group({
    employeeId: [
      '',
      [
        Validators.required,
        Validators.maxLength(50)
      ]
    ],
    name: [
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
      'User' as SignupRole,
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

  setMode(mode: PageMode): void {
    this.mode = mode;
    this.message = '';
    this.messageType = 'error';
    this.showPassword = false;
    this.isLoading = false;

    if (mode === 'signup' && this.departments.length === 0) {
      this.loadDepartments();
    }
  }

  private loadDepartments(): void {
    if (this.isLoadingDepartments) {
      return;
    }

    this.isLoadingDepartments = true;
    this.message = '';

    this.authService
      .getActiveDepartments()
      .pipe(
        finalize(() => {
          this.isLoadingDepartments = false;
        })
      )
      .subscribe({
        next: (response: DepartmentOption[]) => {
          this.departments = response ?? [];

          if (this.departments.length === 0) {
            this.showMessage(
              'No active departments are available.'
            );
          }
        },
        error: (error: HttpErrorResponse) => {
          this.departments = [];

          this.showMessage(
            error.status === 0
              ? 'Cannot connect to the QMS server. Check that the backend is running.'
              : 'Unable to load departments. Please try again.'
          );
        }
      });
  }

  signIn(): void {
    if (this.isLoading) {
      return;
    }

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.showMessage(
        'Enter your employee ID or email and password.'
      );
      return;
    }

    const { username, password } =
      this.loginForm.getRawValue();

    this.isLoading = true;
    this.message = '';
    this.messageType = 'error';

    this.authService
      .login({
        loginId: username.trim(),
        password
      })
      .pipe(
        timeout({ first: 15000 }),
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe({
        next: () => {
          this.isLoading = false;
          void this.router.navigate(['/qms']);
        },
        error: (error: HttpErrorResponse | Error) => {
          // Reset immediately on failure; finalize is an additional safeguard.
          this.isLoading = false;

          if (
            error instanceof HttpErrorResponse &&
            error.status === 401
          ) {
            this.showMessage(
              'Invalid credentials or inactive account.'
            );
          } else if (
            error instanceof HttpErrorResponse &&
            error.status === 400
          ) {
            this.showMessage(
              'Please check your login details and try again.'
            );
          } else if (
            error instanceof HttpErrorResponse &&
            error.status === 0
          ) {
            this.showMessage(
              'Cannot connect to the QMS server. Check that the backend is running.'
            );
          } else if (error.name === 'TimeoutError') {
            this.showMessage(
              'The login request timed out. Check the backend and try again.'
            );
          } else {
            this.showMessage(
              'Sign-in failed. Please try again or contact the QMS administrator.'
            );
          }
        }
      });
  }

  requestPasswordReset(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      this.showMessage('Enter a valid work email address.');
      return;
    }

    this.showMessage(
      'Password reset is not connected yet. Please contact your QMS administrator.'
    );
  }

  requestSignup(): void {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();

      this.showMessage(
        'Complete all fields and meet the password requirements.'
      );
      return;
    }

    if (this.isLoading || this.isLoadingDepartments) {
      return;
    }

    const form = this.signupForm.getRawValue();

    const employeeId = form.employeeId.trim();
    const fullName = form.name.trim();
    const email = form.email.trim();

    if (!employeeId || !fullName || !email || form.departmentId < 1) {
      this.showMessage(
        'Enter valid employee details and select a department.'
      );
      return;
    }

    this.isLoading = true;
    this.message = '';

    this.authService
      .submitSignup({
        employeeId,
        fullName,
        email,
        departmentId: Number(form.departmentId),
        role: form.role,
        password: form.password
      })
      .pipe(
        finalize(() => {
          this.isLoading = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.showMessage(
            response.message ||
              'Your signup request has been submitted for QMS Admin review.',
            'success'
          );

          this.signupForm.reset({
            employeeId: '',
            name: '',
            email: '',
            departmentId: 0,
            role: 'User',
            password: ''
          });
        },
        error: (error: HttpErrorResponse) => {
          if (error.status === 409) {
            this.showMessage(
              error.error?.message ||
                'An account or pending request already exists for these details.'
            );
          } else if (error.status === 0) {
            this.showMessage(
              'Cannot connect to the QMS server. Check that the backend is running.'
            );
          } else if (error.status === 400) {
            this.showMessage(
              error.error?.message ||
                'Please check your details, selected department, and password.'
            );
          } else {
            this.showMessage(
              'Signup failed. Please try again or contact the QMS administrator.'
            );
          }
        }
      });
  }

  private showMessage(
    message: string,
    type: 'error' | 'success' = 'error'
  ): void {
    this.message = message;
    this.messageType = type;
  }
}
