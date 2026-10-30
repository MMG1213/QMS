
import {
  Component,
  HostListener,
  OnInit,
  ChangeDetectorRef,
  ChangeDetectionStrategy,
  inject
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import {
  MasterListService,
  DocumentType,
  Department,
  MasterListDocument,
  DocumentRevision,
  UpdateMasterListDocument
} from '../../services/master-list';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css',
  changeDetection: ChangeDetectionStrategy.Default
})
export class QmsDashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

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

  userName =
    this.currentUser?.fullName ||
    this.currentUser?.employeeId ||
    'QMS User';

  userRole = this.currentUser?.role || 'User';

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

  // Inline editing state
  editingDocumentId: number | null = null;
  editDraft: MasterListDocument | null = null;

  isSavingDocument = false;
  saveMessage = '';
  saveError = '';

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

  // Generate revision columns from the loaded documents
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

  // Load document types and departments
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

  // Load Master List documents
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

  // Document type filter
  onDocumentTypeChange(value: string | number): void {
    this.selectedDocumentTypeId =
      value === '' ? null : Number(value);

    this.loadDocuments();
  }

  // Department filter
  onDepartmentChange(value: string | number): void {
    this.selectedDepartmentId =
      value === '' ? null : Number(value);

    this.loadDocuments();
  }

  // Search documents
  onSearch(): void {
    this.loadDocuments();
  }

  // Reset filters
  clearFilters(): void {
    this.selectedDocumentTypeId = null;
    this.selectedDepartmentId = null;
    this.searchText = '';

    this.loadDocuments();
  }

  // Dashboard navigation
  selectTab(tab: string): void {
    this.activeTab = tab;
    this.profileMenuOpen = false;
  }

  selectDocumentType(type: DocumentType): void {
    this.selectedDocumentTypeId = type.documentTypeId;
    this.loadDocuments();
  }

  // Find a document's revision for a specific revision number
  getRevision(
    document: MasterListDocument,
    revisionNumber: number
  ): DocumentRevision | undefined {
    return document.revisions.find(
      revision => revision.revisionNumber === revisionNumber
    );
  }

  // Get document type name for display
  getDocumentTypeName(documentTypeId: number): string {
    return this.documentTypes.find(
      type => type.documentTypeId === documentTypeId
    )?.typeName ?? '—';
  }

  // Format revision number, e.g. Rev 000
  formatRevision(revisionNumber: number): string {
    return `Rev ${String(revisionNumber).padStart(3, '0')}`;
  }

  // Format date for display
  formatDate(date: string | null | undefined): string {
    if (!date) {
      return '—';
    }

    const datePart = date.substring(0, 10);
    const [year, month, day] = datePart.split('-');

    if (!year || !month || !day) {
      return date;
    }

    return `${day}/${month}/${year}`;
  }

  // Start editing a document
  startEdit(document: MasterListDocument): void {
    if (this.isSavingDocument) {
      return;
    }

    this.editingDocumentId = document.documentId;

    // Create a copy so Cancel does not change the displayed document
    this.editDraft = JSON.parse(
      JSON.stringify(document)
    ) as MasterListDocument;

    this.saveMessage = '';
    this.saveError = '';

    this.cdr.markForCheck();
  }

  // Cancel editing and discard unsaved changes
  cancelEdit(): void {
    if (this.isSavingDocument) {
      return;
    }

    this.editingDocumentId = null;
    this.editDraft = null;

    this.saveMessage = '';
    this.saveError = '';

    this.cdr.markForCheck();
  }

  // Update status and obsolete date together
  onEditStatusChange(status: string): void {
    if (!this.editDraft) {
      return;
    }

    this.editDraft.status = status;

    if (status === 'Active') {
      this.editDraft.obseleteDate = null;
    }

    this.saveError = '';
  }

  // Validate and save the edited document
  saveEdit(): void {
    const draft = this.editDraft;

    if (
      !draft ||
      this.editingDocumentId === null ||
      this.isSavingDocument
    ) {
      return;
    }

    if (!draft.documentNo.trim()) {
      this.saveError = 'Document number is required.';
      return;
    }

    if (!draft.title.trim()) {
      this.saveError = 'Document title is required.';
      return;
    }

    if (!draft.documentTypeId || !draft.departmentId) {
      this.saveError = 'Select a document type and department.';
      return;
    }

    if (
      draft.status !== 'Active' &&
      draft.status !== 'Obsolete'
    ) {
      this.saveError = 'Select a valid document status.';
      return;
    }

    if (
      draft.status === 'Obsolete' &&
      !draft.obseleteDate
    ) {
      this.saveError =
        'Select an obsolete date for obsolete documents.';
      return;
    }

    if (
      draft.revisions.some(revision =>
        !Number.isInteger(Number(revision.pageCount)) ||
        Number(revision.pageCount) < 1 ||
        !revision.revisionDate
      )
    ) {
      this.saveError =
        'Every revision needs a valid page count and date.';
      return;
    }

    const request: UpdateMasterListDocument = {
      documentNo: draft.documentNo.trim(),
      title: draft.title.trim(),
      documentTypeId: Number(draft.documentTypeId),
      departmentId: Number(draft.departmentId),
      retentionPeriod: draft.retentionPeriod?.trim() || null,
      status: draft.status,
      obseleteDate:
        draft.status === 'Obsolete'
          ? draft.obseleteDate?.slice(0, 10) ?? null
          : null,
      revisions: draft.revisions.map(revision => ({
        revisionNumber: revision.revisionNumber,
        pageCount: Number(revision.pageCount),
        revisionDate: revision.revisionDate.slice(0, 10)
      }))
    };

    this.isSavingDocument = true;
    this.saveError = '';
    this.saveMessage = '';

    this.masterListService
      .updateDocument(draft.documentId, request)
      .subscribe({
        next: response => {
          this.isSavingDocument = false;
          this.editingDocumentId = null;
          this.editDraft = null;

          this.saveMessage =
            response?.message ?? 'Document updated successfully.';

          // Reload the saved data from the backend
          this.loadDocuments();
          this.cdr.markForCheck();
        },
        error: error => {
          console.error(
            'Failed to update Master List document:',
            error
          );

          this.isSavingDocument = false;

          this.saveError =
            error?.error?.message ??
            'Could not save changes. Check the entered values and try again.';

          this.cdr.markForCheck();
        }
      });
  }

  // Profile menu
  toggleProfileMenu(): void {
    this.profileMenuOpen = !this.profileMenuOpen;
  }

  closeProfileMenu(): void {
    this.profileMenuOpen = false;
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.closeProfileMenu();

    if (
      this.editingDocumentId !== null &&
      !this.isSavingDocument
    ) {
      this.cancelEdit();
    }
  }

  openProfile(): void {
    this.closeProfileMenu();
    // Add profile navigation here if needed.
  }

  openSignupRequests(): void {
    void this.router.navigate(['/qms/admin/signup-requests']);
  }

  logout(): void {
    this.closeProfileMenu();
    this.authService.logout();

    void this.router.navigate(['/']);
  }
}
