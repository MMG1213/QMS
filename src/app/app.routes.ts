
import { Routes } from '@angular/router';
import { QmsDashboardComponent } from './pages/qms-dashboard/qms-dashboard';

export const routes: Routes = [
  {
    path: 'qms',
    component: QmsDashboardComponent
  },
  {
    path: '',
    redirectTo: 'qms',
    pathMatch: 'full'
  }
];
