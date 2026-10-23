
import { Component, HostListener, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css'
})
export class QmsDashboardComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  openSignupRequests(): void {
    void this.router.navigate(['/qms/admin/signup-requests']);
  }

  tabs: string[] = [
    'MasterList',
    'DCR',
    'Internal Audit',
    'QPI',
    'MRM'
  ];

  activeTab = 'MasterList';
  profileMenuOpen = false;

  private readonly currentUser = this.authService.getCurrentUser();

  userName = this.currentUser?.fullName || this.currentUser?.employeeId || 'QMS User';
  userRole = this.currentUser?.role || 'User';

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
    // Profile page can be implemented separately.
  }

  logout(): void {
    this.closeProfileMenu();
    this.authService.logout();
    void this.router.navigate(['/']);
  }
}
