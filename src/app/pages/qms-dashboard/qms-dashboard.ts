
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

type QmsTab = 'Master List' | 'DCR' | 'Internal Audit' | 'QPM' | 'MRM';

@Component({
  selector: 'app-qms-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css'
})
export class QmsDashboardComponent {
  activeTab: QmsTab = 'Master List';

  tabs: QmsTab[] = [
    'Master List',
    'DCR',
    'Internal Audit',
    'QPM',
    'MRM'
  ];

  selectTab(tab: QmsTab): void {
    this.activeTab = tab;
  }
}
