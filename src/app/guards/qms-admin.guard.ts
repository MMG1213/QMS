
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SigninService } from '../services/signin.service';

export const qmsAdminGuard: CanActivateFn = () => {
  const signinService = inject(SigninService);
  const router = inject(Router);
  const user = signinService.getCurrentUser();

  if (!user) {
    return router.createUrlTree(['/']);
  }

  if (user.role !== 'QMS Admin') {
    return router.createUrlTree(['/qms']);
  }

  return true;
};
