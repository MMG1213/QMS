
import { Routes } from '@angular/router';

import { Login } from './components/login/login';

import { QmsDashboardComponent } from './pages/qms-dashboard/qms-dashboard';
import { MasterListComponent } from './pages/qms-dashboard/master-list/master-list';

// DCR components
import { DcrLayout } from './pages/dcr/dcr-layout/dcr-layout';
import { DcrDashboard } from './pages/dcr/dcr-dashboard/dcr-dashboard';
import { RaiseDcr } from './pages/dcr/raise-dcr/raise-dcr';
import { MyRequests } from './pages/dcr/my-requests/my-requests';
import { PendingApprovals } from './pages/dcr/pending-approvals/pending-approvals';
import { DcrDetails } from './pages/dcr/dcr-details/dcr-details';

import { AdminSignupRequestsComponent } from './components/admin-requests/admin-requests';

import { authGuard } from './guards/signin.guard';
import { qmsAdminGuard } from './guards/qms-admin.guard';

export const routes: Routes = [
  // Login
  {
    path: '',
    component: Login
  },

  // Main QMS application
  {
    path: 'qms',
    component: QmsDashboardComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        redirectTo: 'master-list',
        pathMatch: 'full'
      },

      // Master List
      {
        path: 'master-list',
        component: MasterListComponent
      },

      // Document Change Request
      {
        path: 'dcr',
        component: DcrLayout,
        children: [
          {
            path: '',
            component: DcrDashboard
          },
          {
            path: 'raise',
            component: RaiseDcr
          },
          {
            path: 'my-requests',
            component: MyRequests
          },
          {
            path: 'pending-approvals',
            component: PendingApprovals
          },
          {
            path: 'details/:id',
            component: DcrDetails
          }
        ]
      }
    ]
  },

  // Old signup requests URL redirect
  {
    path: 'qms/admin/signup-requests',
    redirectTo: 'qms/admin/manage-permissions/signup-requests',
    pathMatch: 'full'
  },

  // QMS Admin routes
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
      {
        path: 'manage-department-approval-mappings',
        component: AdminSignupRequestsComponent,
        data: { adminTab: 'approvalMappings' }
      }
    ]
  },

  // Unknown routes
  {
    path: '**',
    redirectTo: ''
  }
];
