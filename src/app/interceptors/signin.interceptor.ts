
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { SigninService } from '../services/signin.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const signinService = inject(SigninService);

  const isApiRequest =
    request.url.startsWith('/api/') ||
    request.url.startsWith('http://localhost:5193/api/');

  if (!isApiRequest) {
    return next(request);
  }

  const token = signinService.getToken();

  if (!token) {
    return next(request);
  }

  return next(
    request.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    })
  );
};
