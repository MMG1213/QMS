
import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { QmsDashboardComponent } from './pages/qms-dashboard/qms-dashboard';
import { AdminSignupRequestsComponent } from './components/admin-signup-requests/admin-signup-requests';
import { authGuard } from './guards/signin.guard';
import { qmsAdminGuard } from './guards/qms-admin.guard';

export const routes: Routes = [
  {
    path: '',
    component: Login
  },
  {
    path: 'qms',
    component: QmsDashboardComponent,
    canActivate: [authGuard]
  },
  {
    path: 'qms/admin/signup-requests',
    component: AdminSignupRequestsComponent,
    canActivate: [authGuard, qmsAdminGuard]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
