
import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { QmsDashboardComponent } from './pages/qms-dashboard/qms-dashboard';
import { AdminSignupRequestsComponent } from './components/admin-requests/admin-requests';
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
    redirectTo: 'qms/admin/manage-permissions/signup-requests',
    pathMatch: 'full'
  },
  {
    path: 'qms/admin/manage-permissions',
    canActivate: [authGuard, qmsAdminGuard],
    children: [
      {
        path: '',
        redirectTo: 'manage-signup-requests',
        pathMatch: 'full'
      },
      {
        path: 'manage-signup-requests',
        component: AdminSignupRequestsComponent,
        data: { adminTab: 'requests' }
      },
      {
        path: 'manage-user-access',
        component: AdminSignupRequestsComponent,
        data: { adminTab: 'access' }
      },
      {
        path: 'manage-departments',
        component: AdminSignupRequestsComponent,
        data: { adminTab: 'departments' }
      },
      {
        path: 'manage-document-types',
        component: AdminSignupRequestsComponent,
        data: { adminTab: 'documentTypes' }
      },
      { path: 'manage-department-approval-mappings',
        component: AdminSignupRequestsComponent,
        data: { adminTab: 'approvalMappings' } 
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
