import { Routes } from '@angular/router';
import { Login } from './components/login/login';
import { QmsDashboardComponent } from './pages/qms-dashboard/qms-dashboard';

export const routes: Routes = [
  {
    path: '',
    component: Login
  },
  {
    path: 'qms',
    component: QmsDashboardComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
