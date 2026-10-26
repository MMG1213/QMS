
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const qmsAdminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const user = authService.getCurrentUser();

  if (!user) {
    return router.createUrlTree(['/']);
  }

  if (user.role !== 'QMS Admin') {
    return router.createUrlTree(['/qms']);
  }

  return true;
};
