
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

import {
  MasterDataService,
  MasterDataTable,
  MasterDataRow
} from '../../services/master-data.service';

import { SigninService } from '../../services/signin.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qms-dashboard.html',
  styleUrl: './qms-dashboard.css',
  changeDetection: ChangeDetectionStrategy.Default
})
export class QmsDashboardComponent implements OnInit {

  private readonly SigninService = inject(SigninService);
  private readonly router = inject(Router);
  private readonly masterDataService = inject(MasterDataService);

  constructor(
    private readonly masterListService: MasterListService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  // --------------------------------------------------
  // DASHBOARD
  // --------------------------------------------------

  tabs: string[] = [
    'MasterList',
    'DCR',
    'Internal Audit',
    'QPI',
    'MRM'
  ];

  activeTab = 'MasterList';
  profileMenuOpen = false;

  private readonly currentUser = this.SigninService.getCurrentUser();

  userName =
    this.currentUser?.fullName ||
    this.currentUser?.employeeId ||
    'QMS User';

  userRole = this.currentUser?.role || 'User';

  // Only QMS Admin can edit Master List records.
  isQmsAdmin = this.userRole === 'QMS Admin';

  get userInitial(): string {
    return this.userName.trim().charAt(0).toUpperCase() || 'U';
  }

  // --------------------------------------------------
  // MASTER LIST DATA
  // --------------------------------------------------

  documentTypes: DocumentType[] = [];
  departments: Department[] = [];
  documents: MasterListDocument[] = [];

  selectedDocumentTypeId: number | null = null;
  selectedDepartmentId: number | null = null;
  searchText = '';

  isLoadingFilters = false;
  isLoadingDocuments = false;
  isLoadingMasterData = false;

  filterError = '';
  documentError = '';
  masterDataError = '';

  // --------------------------------------------------
  // ML02–ML06 DOCUMENT EDITING
  // --------------------------------------------------

  editingDocumentId: number | null = null;
  editDraft: MasterListDocument | null = null;

  isSavingDocument = false;
  saveMessage = '';
  saveError = '';

  // --------------------------------------------------
  // ML07–ML10 SPECIAL TABLE DATA
  // --------------------------------------------------

  masterDataRows: MasterDataRow[] = [];

  editingMasterDataId: number | null = null;
  specialEditDraft: MasterDataRow | null = null;

  specialSaveMessage = '';
  specialSaveError = '';

  // --------------------------------------------------
  // SPECIAL DOCUMENT TYPE CONFIGURATION
  // --------------------------------------------------

  readonly specialDocumentTypes: Record<number, {
    table: MasterDataTable;
    title: string;
    columns: { key: string; label: string }[];
  }> = {

    // ML07 - Machines / Equipment
    6: {
      table: 'machines',
      title: 'Machines / Equipment',
      columns: [
        { key: 'machineName', label: 'Machine Name' },
        { key: 'machineCategory', label: 'Machine Category' },
        { key: 'machineCodeNum', label: 'Machine Code No.' },
        { key: 'machineSlNum', label: 'Machine Serial No.' },
        { key: 'make', label: 'Make' },
        { key: 'capacityOrSpecification', label: 'Capacity / Specification' },
        { key: 'commissionedOn', label: 'Commissioned On' },
        { key: 'loadInHP', label: 'Load in HP' },
        { key: 'location', label: 'Location' }
      ]
    },

    // ML08 - Standards
    7: {
      table: 'standards',
      title: 'Standards',
      columns: [
        { key: 'description', label: 'Description' },
        { key: 'pages', label: 'Pages' },
        { key: 'revision', label: 'Revision' },
        { key: 'remarks', label: 'Remarks' }
      ]
    },

    // ML09 - Testing and Measuring Instruments
    8: {
      table: 'instruments',
      title: 'Testing and Measuring Instruments',
      columns: [
        { key: 'instrumentDescription', label: 'Instrument Description' },
        { key: 'instrumentCodeNum', label: 'Instrument Code No.' },
        { key: 'instrumentSlNum', label: 'Instrument Serial No.' },
        { key: 'instrumentRange', label: 'Range' },
        { key: 'make', label: 'Make' },
        { key: 'location', label: 'Location' },
        { key: 'calibrationFrequency', label: 'Calibration Frequency' },
        { key: 'remarks', label: 'Remarks' }
      ]
    },

    // ML10 - Quality Performance Index
    9: {
      table: 'qpi',
      title: 'Quality Performance Index (QPI)',
      columns: [
        { key: 'processOrDept', label: 'Process / Department' },
        { key: 'measurementTitle', label: 'Measurement Title' },
        { key: 'qualityPerformanceIndexFormula', label: 'QPI Formula' },
        { key: 'targets', label: 'Targets' },
        { key: 'responsibilities', label: 'Responsibilities' },
        { key: 'departmentReview', label: 'Department Review' },
        { key: 'qpiReview', label: 'QPI Review' }
      ]
    }
  };

  // --------------------------------------------------
  // SELECTED SPECIAL TABLE CONFIGURATION
  // --------------------------------------------------

  get selectedSpecialType(): {
    table: MasterDataTable;
    title: string;
    columns: { key: string; label: string }[];
  } | null {

    if (this.selectedDocumentTypeId === null) {
      return null;
    }

    return this.specialDocumentTypes[this.selectedDocumentTypeId] ?? null;
  }

  // --------------------------------------------------
  // REVISION COLUMNS
  // --------------------------------------------------

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

  // --------------------------------------------------
  // INITIALIZATION
  // --------------------------------------------------

  ngOnInit(): void {
    this.loadFilters();
    this.loadDocuments();
  }

  // --------------------------------------------------
  // LOAD FILTERS
  // --------------------------------------------------

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

  // --------------------------------------------------
  // LOAD ML02–ML06 DOCUMENTS
  // --------------------------------------------------

  loadDocuments(): void {

    if (this.selectedSpecialType) {

      this.documents = [];
      this.isLoadingDocuments = false;
      this.documentError = '';

      this.cdr.markForCheck();
      return;
    }

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

  // --------------------------------------------------
  // LOAD ML07–ML10 SPECIAL TABLES
  // --------------------------------------------------

  loadMasterData(): void {

    const config = this.selectedSpecialType;

    this.masterDataRows = [];
    this.masterDataError = '';

    this.editingMasterDataId = null;
    this.specialEditDraft = null;

    if (!config) {

      this.isLoadingMasterData = false;

      this.cdr.markForCheck();
      return;
    }

    this.isLoadingMasterData = true;

    this.cdr.markForCheck();

    this.masterDataService.getRows(config.table).subscribe({

      next: rows => {

        if (this.selectedSpecialType?.table !== config.table) {
          return;
        }

        this.masterDataRows = rows;
        this.isLoadingMasterData = false;

        this.cdr.markForCheck();
      },

      error: error => {

        console.error('Failed to load master data:', error);

        if (this.selectedSpecialType?.table !== config.table) {
          return;
        }

        this.masterDataRows = [];

        this.masterDataError =
          'Could not load the selected master data. Please try again.';

        this.isLoadingMasterData = false;

        this.cdr.markForCheck();
      }
    });
  }

  // --------------------------------------------------
  // DOCUMENT TYPE FILTER
  // --------------------------------------------------

  onDocumentTypeChange(value: string | number): void {

    this.selectedDocumentTypeId =
      value === '' ? null : Number(value);

    this.editingDocumentId = null;
    this.editDraft = null;
    this.saveMessage = '';
    this.saveError = '';

    this.editingMasterDataId = null;
    this.specialEditDraft = null;
    this.specialSaveMessage = '';
    this.specialSaveError = '';

    if (this.selectedSpecialType) {
      this.selectedDepartmentId = null;
    }

    this.loadDocuments();
    this.loadMasterData();
  }

  // --------------------------------------------------
  // DEPARTMENT FILTER
  // --------------------------------------------------

  onDepartmentChange(value: string | number): void {

    if (this.selectedSpecialType) {
      return;
    }

    this.selectedDepartmentId =
      value === '' ? null : Number(value);

    this.loadDocuments();
  }

  // --------------------------------------------------
  // SEARCH
  // --------------------------------------------------

  onSearch(): void {
    this.loadDocuments();
  }

  // --------------------------------------------------
  // REFRESH
  // --------------------------------------------------

  refreshCurrentTable(): void {

    this.loadDocuments();
    this.loadMasterData();
  }

  // --------------------------------------------------
  // CLEAR FILTERS
  // --------------------------------------------------

  clearFilters(): void {

    this.selectedDocumentTypeId = null;
    this.selectedDepartmentId = null;
    this.searchText = '';

    this.editingDocumentId = null;
    this.editDraft = null;

    this.editingMasterDataId = null;
    this.specialEditDraft = null;

    this.saveMessage = '';
    this.saveError = '';

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.loadDocuments();
    this.loadMasterData();
  }

  // --------------------------------------------------
  // DASHBOARD NAVIGATION
  // --------------------------------------------------

  selectTab(tab: string): void {

    this.activeTab = tab;
    this.profileMenuOpen = false;
  }

  selectDocumentType(type: DocumentType): void {

    this.onDocumentTypeChange(type.documentTypeId);
  }

  // --------------------------------------------------
  // SPECIAL TABLE EDITING - ML07–ML10
  // FRONTEND ONLY FOR NOW
  // --------------------------------------------------

  startSpecialEdit(row: MasterDataRow): void {

    // Only QMS Admin can start editing.
    if (!this.isQmsAdmin) {
      return;
    }

    if (
      this.editingMasterDataId !== null ||
      this.editingDocumentId !== null ||
      this.isSavingDocument
    ) {
      return;
    }

    this.editingMasterDataId = row.id;

    this.specialEditDraft = JSON.parse(
      JSON.stringify(row)
    ) as MasterDataRow;

    if (this.specialEditDraft['commissionedOn']) {

      this.specialEditDraft['commissionedOn'] =
        String(this.specialEditDraft['commissionedOn']).substring(0, 10);
    }

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.cdr.markForCheck();
  }

  cancelSpecialEdit(): void {

    this.editingMasterDataId = null;
    this.specialEditDraft = null;

    this.specialSaveMessage = '';
    this.specialSaveError = '';

    this.cdr.markForCheck();
  }

  getSpecialInputType(key: string): string {

    if (key === 'commissionedOn') {
      return 'date';
    }

    if (key === 'pages' || key === 'loadInHP') {
      return 'number';
    }

    return 'text';
  }

  updateSpecialDraftField(key: string, value: any): void {

    if (!this.specialEditDraft) {
      return;
    }

    if (key === 'pages' || key === 'loadInHP') {

      this.specialEditDraft[key] =
        value === '' ? null : Number(value);

    } else {

      this.specialEditDraft[key] = value;
    }
  }

  saveSpecialEdit(): void {

    if (
      !this.isQmsAdmin ||
      !this.specialEditDraft ||
      this.editingMasterDataId === null
    ) {
      return;
    }

    const draft = { ...this.specialEditDraft };

    const index = this.masterDataRows.findIndex(
      row => row.id === this.editingMasterDataId
    );

    if (index === -1) {

      this.specialSaveError = 'Record not found.';
      return;
    }

    this.masterDataRows = this.masterDataRows.map(
      (row, i) => i === index ? draft : row
    );

    this.editingMasterDataId = null;
    this.specialEditDraft = null;

    this.specialSaveError = '';

    this.specialSaveMessage =
      'Changes updated in this session only. They are not saved to the database.';

    this.cdr.markForCheck();
  }

  // --------------------------------------------------
  // GET DOCUMENT REVISION
  // --------------------------------------------------

  getRevision(
    document: MasterListDocument,
    revisionNumber: number
  ): DocumentRevision | undefined {

    return document.revisions.find(
      revision => revision.revisionNumber === revisionNumber
    );
  }

  // --------------------------------------------------
  // DOCUMENT TYPE NAME
  // --------------------------------------------------

  getDocumentTypeName(documentTypeId: number): string {

    return this.documentTypes.find(
      type => type.documentTypeId === documentTypeId
    )?.typeName ?? '—';
  }

  // --------------------------------------------------
  // REVISION FORMATTING
  // --------------------------------------------------

  formatRevision(revisionNumber: number): string {

    return `Rev ${String(revisionNumber).padStart(3, '0')}`;
  }

  // --------------------------------------------------
  // DATE FORMATTING
  // --------------------------------------------------

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

  // --------------------------------------------------
  // ML02–ML06 DOCUMENT EDITING
  // --------------------------------------------------

  startEdit(document: MasterListDocument): void {

    if (!this.isQmsAdmin) {
      return;
    }
  
    if (
      this.editingDocumentId !== null ||
      this.editingMasterDataId !== null ||
      this.isSavingDocument
    ) {
      return;
    }
  
    console.log('Original Document:', document);
  
    this.editingDocumentId = document.documentId;
  
    this.editDraft = {
      ...document,
  
      // Convert IDs to numbers for [ngValue] dropdowns
      documentTypeId: Number(document.documentTypeId),
      departmentId: Number(document.departmentId),
  
      // Copy revisions and normalize dates
      revisions: (document.revisions ?? []).map(revision => ({
        ...revision,
        revisionDate: revision.revisionDate
          ? String(revision.revisionDate).substring(0, 10)
          : ''
      })),
  
      // Normalize obsolete date
      obseleteDate: document.obseleteDate
        ? String(document.obseleteDate).substring(0, 10)
        : null
    };
  
    console.log('Edit Draft:', this.editDraft);
  
    this.cdr.detectChanges();
  }

  // --------------------------------------------------
  // CANCEL DOCUMENT EDITING
  // --------------------------------------------------

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

  // --------------------------------------------------
  // DOCUMENT STATUS CHANGE
  // --------------------------------------------------

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

  // --------------------------------------------------
  // SAVE ML02–ML06 DOCUMENT
  // BACKEND DATABASE SAVE
  // --------------------------------------------------

  saveEdit(): void {

    const draft = this.editDraft;

    if (
      !this.isQmsAdmin ||
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

  // --------------------------------------------------
  // PROFILE MENU
  // --------------------------------------------------

  toggleProfileMenu(): void {

    this.profileMenuOpen = !this.profileMenuOpen;
  }

  closeProfileMenu(): void {

    this.profileMenuOpen = false;
  }

  openProfile(): void {

    this.closeProfileMenu();
  }

  // --------------------------------------------------
  // SIGNUP REQUESTS
  // --------------------------------------------------

  openSignupRequests(): void {

    void this.router.navigate(['/qms/admin/signup-requests']);
  }

  // --------------------------------------------------
  // KEYBOARD ESCAPE
  // --------------------------------------------------

  @HostListener('document:keydown.escape')
  onEscape(): void {

    this.closeProfileMenu();

    if (this.editingMasterDataId !== null) {

      this.cancelSpecialEdit();
      return;
    }

    if (
      this.editingDocumentId !== null &&
      !this.isSavingDocument
    ) {

      this.cancelEdit();
    }
  }

  // --------------------------------------------------
  // LOGOUT
  // --------------------------------------------------

  logout(): void {

    this.closeProfileMenu();

    this.SigninService.logout();

    void this.router.navigate(['/']);
  }
}
