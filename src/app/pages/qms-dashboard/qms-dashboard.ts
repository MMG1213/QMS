
import { Component, HostListener } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css'
})
export class QmsDashboardComponent {
  tabs: string[] = [
    'MasterList',
    'DCR',
    'Internal Audit',
    'QPI',
    'MRM'
  ];

  activeTab = 'MasterList';
  profileMenuOpen = false;

  // Replace these demo values with logged-in user data.
  userName = 'QMS User';
  userRole = 'Quality Department';

  get userInitial(): string {
    return this.userName.trim().charAt(0).toUpperCase() || 'U';
  }

  selectTab(tab: string): void {
    this.activeTab = tab;
    this.profileMenuOpen = false;
  }

  toggleProfileMenu(): void {
    this.profileMenuOpen = !this.profileMenuOpen;
  }

  closeProfileMenu(): void {
    this.profileMenuOpen = false;
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closeProfileMenu();
  }

  openProfile(): void {
    this.closeProfileMenu();

    // TODO: Navigate to your profile page when implemented.
    console.log('Open user profile');
  }

  logout(): void {
    this.closeProfileMenu();

    // TODO: Call your ASP.NET Core logout endpoint,
    // clear authentication state, and navigate to login.
    console.log('Logout requested');
  }
}
