
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SigninService } from '../services/signin.service';

export const authGuard: CanActivateFn = () => {
  const signinService = inject(SigninService);
  const router = inject(Router);

  if (signinService.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/']);
};
