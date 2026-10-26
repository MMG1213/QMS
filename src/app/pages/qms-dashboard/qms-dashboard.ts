
import {
  Component,
  HostListener,
  OnInit,
  ChangeDetectorRef,
  ChangeDetectionStrategy
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  MasterListService,
  DocumentType,
  Department,
  MasterListDocument,
  DocumentRevision
} from '../../services/master-list';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css',
  changeDetection: ChangeDetectionStrategy.Default
})
export class QmsDashboardComponent implements OnInit {
  tabs: string[] = [
    'MasterList',
    'DCR',
    'Internal Audit',
    'QPI',
    'MRM'
  ];

  activeTab = 'MasterList';
  profileMenuOpen = false;

  userName = 'QMS User';
  userRole = 'Quality Department';

  documentTypes: DocumentType[] = [];
  departments: Department[] = [];
  documents: MasterListDocument[] = [];

  selectedDocumentTypeId: number | null = null;
  selectedDepartmentId: number | null = null;
  searchText = '';

  isLoadingFilters = false;
  isLoadingDocuments = false;

  filterError = '';
  documentError = '';

  constructor(
    private readonly masterListService: MasterListService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadFilters();
    this.loadDocuments();
  }

  get userInitial(): string {
    return this.userName.trim().charAt(0).toUpperCase() || 'U';
  }

  get revisionColumns(): number[] {
    const revisionNumbers = this.documents.flatMap(document =>
      document.revisions.map(revision => revision.revisionNumber)
    );

    if (revisionNumbers.length === 0) {
      return [];
    }

    const highestRevision = Math.max(...revisionNumbers);
    const lowestRevision = Math.min(0, ...revisionNumbers);

    return Array.from(
      { length: highestRevision - lowestRevision + 1 },
      (_, index) => lowestRevision + index
    );
  }

  loadFilters(): void {
    this.isLoadingFilters = true;
    this.filterError = '';

    let typesLoaded = false;
    let departmentsLoaded = false;

    const finishLoading = (): void => {
      if (typesLoaded && departmentsLoaded) {
        this.isLoadingFilters = false;
      }

      this.cdr.markForCheck();
    };

    this.masterListService.getDocumentTypes().subscribe({
      next: types => {
        this.documentTypes = types;
        typesLoaded = true;
        finishLoading();
      },
      error: error => {
        console.error('Failed to load document types:', error);

        this.filterError = 'Could not load document types.';
        typesLoaded = true;
        finishLoading();
      }
    });

    this.masterListService.getDepartments().subscribe({
      next: departments => {
        this.departments = departments;
        departmentsLoaded = true;
        finishLoading();
      },
      error: error => {
        console.error('Failed to load departments:', error);

        this.filterError = 'Could not load departments.';
        departmentsLoaded = true;
        finishLoading();
      }
    });
  }

  loadDocuments(): void {
    this.isLoadingDocuments = true;
    this.documentError = '';

    this.cdr.markForCheck();

    this.masterListService.getDocuments(
      this.selectedDocumentTypeId ?? undefined,
      this.selectedDepartmentId ?? undefined,
      this.searchText
    ).subscribe({
      next: documents => {
        this.documents = documents;
        this.isLoadingDocuments = false;

        this.cdr.markForCheck();
      },
      error: error => {
        console.error('Failed to load documents:', error);

        this.documents = [];
        this.documentError =
          'Could not load documents. Check that the backend API is running.';
        this.isLoadingDocuments = false;

        this.cdr.markForCheck();
      }
    });
  }

  onDocumentTypeChange(value: string | number): void {
    this.selectedDocumentTypeId =
      value === '' ? null : Number(value);

    this.loadDocuments();
  }

  onDepartmentChange(value: string | number): void {
    this.selectedDepartmentId =
      value === '' ? null : Number(value);

    this.loadDocuments();
  }

  onSearch(): void {
    this.loadDocuments();
  }

  clearFilters(): void {
    this.selectedDocumentTypeId = null;
    this.selectedDepartmentId = null;
    this.searchText = '';

    this.loadDocuments();
  }

  selectTab(tab: string): void {
    this.activeTab = tab;
    this.profileMenuOpen = false;
  }

  selectDocumentType(type: DocumentType): void {
    this.selectedDocumentTypeId = type.documentTypeId;
    this.loadDocuments();
  }

  getRevision(
    document: MasterListDocument,
    revisionNumber: number
  ): DocumentRevision | undefined {
    return document.revisions.find(
      revision => revision.revisionNumber === revisionNumber
    );
  }

  formatRevision(revisionNumber: number): string {
    return `Rev ${String(revisionNumber).padStart(3, '0')}`;
  }

  formatDate(date: string | null | undefined): string {
    if (!date) {
      return '—';
    }

    const parsedDate = new Date(date);

    if (Number.isNaN(parsedDate.getTime())) {
      return date;
    }

    return new Intl.DateTimeFormat('en-GB', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      timeZone: 'UTC'
    }).format(parsedDate);
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
    console.log('Open user profile');
  }

  logout(): void {
    this.closeProfileMenu();
    console.log('Logout requested');
  }
}
