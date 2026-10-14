
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

type PageMode = 'login' | 'forgot' | 'signup';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  mode: PageMode = 'login';
  showPassword = false;
  message = '';
  messageType: 'error' | 'success' = 'error';

  loginForm;
  forgotForm;
  signupForm;

  constructor(private fb: FormBuilder) {
    this.loginForm = this.fb.nonNullable.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.forgotForm = this.fb.nonNullable.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.signupForm = this.fb.nonNullable.group({
      employeeId: ['', Validators.required],
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  setMode(mode: PageMode): void {
    this.mode = mode;
    this.message = '';
    this.showPassword = false;
  }

  signIn(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.showMessage('Enter your username and password.');
      return;
    }

    this.showMessage(
      'The form is valid. Connect your authentication API to sign in.',
      'success'
    );
  }

  requestPasswordReset(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      this.showMessage('Enter a valid work email address.');
      return;
    }

    this.showMessage(
      'The email is valid. Password reset requires a backend service.',
      'success'
    );
  }

  requestSignup(): void {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      this.showMessage(
        'Complete all fields. Your password must contain at least 8 characters.'
      );
      return;
    }

    this.showMessage(
      'The form is valid. Connect the registration API to request QMS access.',
      'success'
    );
  }

  private showMessage(
    message: string,
    type: 'error' | 'success' = 'error'
  ): void {
    this.message = message;
    this.messageType = type;
  }
}
